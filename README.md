
# Welcome to DracoArts

![Logo](https://dracoarts-logo.s3.eu-north-1.amazonaws.com/DracoArts.png)



# ERC20 Token Transfers in Unity Using Nethereum

This guide provides an in-depth explanation of how to integrate ERC20 token transfers into a Unity application using Nethereum, a powerful .NET library for interacting with the Ethereum blockchain. The implementation allows Unity-based games, apps, or decentralized applications (dApps) to:

- Query token balances of Ethereum addresses.

- Transfer ERC20 tokens between wallets.

- Interact with smart contracts securely and efficiently.

## 1. Nethereum Library
#### Nethereum serves as the bridge between Unity and the Ethereum blockchain, providing:

- Web3 functionality for connecting to Ethereum nodes

- Contract interaction capabilities

- Transaction creation and signing

- Event listening and querying

## 2. ERC20 Token Standard Implementation
### The solution implements the essential ERC20 standard functions:

- transfer: Moves tokens from sender to recipient

- balanceOf: Queries token balance of an address

- decimals: Retrieves token decimal precision

-symbol: Gets token symbol (optional)

## Workflow Architecture
### 1. Initialization Phase

- Web3 Connection: Establishes connection to an Ethereum node via Infura, Alchemy, or local node

- Contract Binding: Loads the ERC20 token contract using its ABI (Application Binary Interface) and address

- Wallet Setup: Configures the sender's Ethereum address and private key for transaction signing

### 2. Balance Query Process
- Creates a balanceOf function call with the target address

- Queries the contract for token balance (in raw units)

- Retrieves token decimals to convert raw balance to human-readable format

- Returns the formatted balance

### 3. Token Transfer Process
### Amount Conversion:

- Retrieves token decimals from contract

- Converts human-readable amount to token units (considering decimals)

### Transaction Preparation:

- Creates transfer function call with recipient address and converted amount

- Estimates gas requirements for the transaction

### Transaction Execution:

- Signs the transaction with sender's private key

- Broadcasts the signed transaction to the network

- Waits for transaction mining confirmation

### Result Handling:

- Returns transaction hash upon successful broadcast

- Provides transaction receipt after mining confirmation

- Handles and reports any errors during the process

### Performance Optimization
### Caching Strategies
- Token decimals caching to reduce redundant queries

- Balance caching with refresh mechanisms

- Contract instance reuse

### Network Optimization
- Batch requests where possible

- Optimal gas price strategies

- Network timeout configurations

# Technical Considerations
### Security Implementation
- Private key management with secure storage considerations

- Transaction signing process without exposing sensitive information

- Secure communication with Ethereum node providers

### Blockchain Interaction Patterns
- Asynchronous operations for all blockchain interactions

- Proper gas estimation and price calculation

- Transaction lifecycle management (pending, confirmed, failed states)

### Unity Integration Requirements
- Coroutine or async/await patterns for non-blocking UI

- Main thread consideration for Unity API calls

- Error handling compatible with Unity's execution environment

#  Prerequisites
- Unity 2019.4 or later

- Nethereum Unity package

- Web3 provider (Infura, Alchemy, or local node)

- ERC20 token contract address

- Ethereum wallet with private key

## Setup
 -  Install Nethereum
- Add Nethereum to your Unity project by:

- Downloading the Nethereum Unity package from GitHub

- Importing the package into your Unity project (Assets > Import Package > Custom Package)

## Usage/Examples
    
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
## Images

 Erc20tokenTransfer
![](https://github.com/AzharKhemta/Gif-File-images/blob/main/Erc20tokenTransfer.gif?raw=true)
## Authors

- [@MirHamzaHasan](https://github.com/MirHamzaHasan)
- [@WebSite](https://mirhamzahasan.com)


## 🔗 Links

[![linkedin](https://img.shields.io/badge/linkedin-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/company/mir-hamza-hasan/posts/?feedView=all/)
## Documentation

[Nethereum ERC20Token](https://docs.nethereum.com/en/latest/nethereum-smartcontrats-gettingstarted/)




## Tech Stack
**Client:** Unity,C#

**Plugin:** Nethereum.Unity 



