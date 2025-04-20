using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using UnityEngine.UI;

public class ERC20Manager : MonoBehaviour
{
    [Header("Configuration")]
    public string infuraUrl = "https://mainnet.infura.io/v3/YOUR_INFURA_PROJECT_ID";
    public string contractAddress = "0xTokenContractAddress";

    [Header("UI Balance")]
    public InputField addressInputField;
    //  This Private Key is just for testing ;
    [SerializeField]
    private string privateKey;
    public Text balanceText;
    public Text statusText;
    public Button checkBalanceButton;
    [Header("UI Transfer")]
    public InputField recipientInputField;
    public InputField amountInputField;
    public Button copyButton;
    public Button transferButton;

    public Text TransctionHashText;
    public GameObject loadingScreen;


    
    // Standard ERC-20 ABI (simplified for transfer and balance)
    public static string ERC20_ABI = @"[{
        'constant':false,
        'inputs':[
            {'name':'_to','type':'address'},
            {'name':'_value','type':'uint256'}],
        'name':'transfer',
        'outputs':[{'name':'','type':'bool'}],
        'type':'function'
    },
    {
        'constant':true,
        'inputs':[{'name':'_owner','type':'address'}],
        'name':'balanceOf',
        'outputs':[{'name':'','type':'uint256'}],
        'type':'function'
    },
    {
        'constant':true,
        'inputs':[],
        'name':'decimals',
        'outputs':[{'name':'','type':'uint8'}],
        'type':'function'
    },
    {
        'constant':true,
        'inputs':[],
        'name':'symbol',
        'outputs':[{'name':'','type':'string'}],
        'type':'function'
    }]";

    private Web3 web3;
    private Contract contract;
    private string tokenSymbol;
    private int decimals;

    private void Start()
    {
        transferButton.onClick.AddListener(OnTransferButtonClicked);
        checkBalanceButton.onClick.AddListener(OnCheckBalanceClicked);
        copyButton.onClick.AddListener(CopyPhraseToClipboard);
        InitializeWeb3();
    }

    private async void InitializeWeb3()
    {
        try
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                UpdateStatus("Please enter your private key");
                return;
            }

            var account = new Account(privateKey);
            web3 = new Web3(account, infuraUrl);
            contract = web3.Eth.GetContract(ERC20_ABI, contractAddress);

            // Get token info once
            var symbolFunction = contract.GetFunction("symbol");
            tokenSymbol = await symbolFunction.CallAsync<string>();

            var decimalsFunction = contract.GetFunction("decimals");
            decimals = await decimalsFunction.CallAsync<int>();

            UpdateStatus($"Connected to {tokenSymbol} contract");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Initialization error: {ex.Message}");
        }
    }

    public async void OnCheckBalanceClicked()
    {
        try
        {


            if (string.IsNullOrEmpty(addressInputField.text))
            {
                UpdateStatus("Please enter an address to check");
                return;
            }
            UpdateStatus("Checking balance...");

            var balanceFunction = contract.GetFunction("balanceOf");
            var balance = await balanceFunction.CallAsync<BigInteger>(addressInputField.text);

            // Format balance with decimals
            float formattedBalance = (float)balance / Mathf.Pow(10, decimals);
            balanceText.text = $"{formattedBalance} {tokenSymbol}";
          
            UpdateStatus($"Balance checked for {addressInputField.text}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Balance check error: {ex.Message}");
        }
    }

    public async void OnTransferButtonClicked()
    {
        try
        {
            if (!float.TryParse(amountInputField.text, out float amount))
            {
                UpdateStatus("Invalid amount");
                return;
            }

            if (string.IsNullOrEmpty(recipientInputField.text))
            {
                UpdateStatus("Please enter recipient address");
                return;
            }

            loadingScreen.SetActive(true);
            UpdateStatus("Preparing transfer...");

            // Calculate token amount in smallest units
            BigInteger tokenAmount = (BigInteger)(amount * Mathf.Pow(10, decimals));

            // Get gas estimate with buffer (important!)
            var transferFunction = contract.GetFunction("transfer");

            // First check ETH balance for gas
            var ethBalance = await web3.Eth.GetBalance.SendRequestAsync(addressInputField.text);
            if (ethBalance.Value == 0)
            {
                UpdateStatus("Insufficient ETH for gas fees");
                loadingScreen.SetActive(false);
                return;
            }

            // Estimate gas with 30% buffer to prevent underestimation
            // Estimate gas
            var gasEstimate = await transferFunction.EstimateGasAsync(
                from: addressInputField.text,
                gas: null,
                value: new HexBigInteger(0),
                functionInput: new object[] { recipientInputField.text, tokenAmount });

            // Calculate gas with 30% buffer (using only BigInteger operations)
            BigInteger gasEstimateValue = gasEstimate.Value;
            BigInteger buffer = gasEstimateValue / 10 * 3; // 30% of the estimate
            BigInteger gasWithBufferValue = gasEstimateValue + buffer;
            var gasWithBuffer = new HexBigInteger(gasWithBufferValue);
         
            // Get current gas price
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            // Check if ETH balance can cover gas costs
            var totalGasCost = gasPrice.Value * gasWithBuffer.Value;
            if (ethBalance.Value < totalGasCost)
            {
                UpdateStatus($"Insufficient ETH for gas. Needed: {Web3.Convert.FromWei(totalGasCost)} ETH");
                loadingScreen.SetActive(false);
                return;
            }

            // Perform transfer with gas buffer
            UpdateStatus("Sending transaction...");
            var txHash = await transferFunction.SendTransactionAsync(
                from: addressInputField.text,
                gas: gasWithBuffer,
                gasPrice: gasPrice,
                value: new HexBigInteger(0),
                functionInput: new object[] { recipientInputField.text, tokenAmount });
       
            UpdateStatus("Transaction sent!");
             amountInputField.text="";
            TransctionHashText.text = txHash;
            // After getting txHash, add this code to check transaction status:
            // try
            // {
            //     var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            //     int attempts = 0;

            //     while (receipt == null && attempts < 12) // Wait up to 1 minute (12x5sec)
            //     {
            //         await Task.Delay(5000); // Wait 5 seconds
            //         receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            //         attempts++;
            //         UpdateStatus($"Checking transaction... Attempt {attempts}");
            //     }

            //     if (receipt != null)
            //     {
            //         UpdateStatus($"Transaction mined in block {receipt.BlockNumber.Value}");
            //         if (receipt.Status.Value == 0){
            //             UpdateStatus("Transaction failed (out of gas or reverted)");
            //         }
            //         else{
            //             UpdateStatus("Transaction succeeded!");
            //              loadingScreen.SetActive(false);}
            //     }
            //     else
            //     {
            //         UpdateStatus("Transaction not found after 1 minute. It may still be pending.");
            //     }
            // }
            // catch (Exception ex)
            // {
            //     UpdateStatus($"Error checking transaction: {ex.Message}");
            // }

            // Check balance after transfer (with delay)
        
        }
        catch (Exception ex)
        {
            UpdateStatus($"Transfer error: {ex.Message}");
            loadingScreen.SetActive(false);
        }
        finally
        {
            loadingScreen.SetActive(false);
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }

    public void CopyPhraseToClipboard()
    {
        if (!string.IsNullOrEmpty(TransctionHashText.text))
        {
            GUIUtility.systemCopyBuffer = TransctionHashText.text;
            statusText.text = "Hash copied to clipboard!";
        }
    }
       
      

}