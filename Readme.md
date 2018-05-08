# CryptoPrices Executable

### Usage:
Edit App.config with the settings specified below & run the tool

### Settings:

* **RefreshRateInSeconds** - How often the output will refresh (minimum of 30 seconds)

* **EnableTA** - Whether or not to show the Technical Analysis
* **AssessTA** - Will display the buy/sell/hodl recommendation based on your settings  
* **TradeSignal##** - The trade signals  Format {coinName, pairedWith, exchange, entryTarget, target1, target2, stopLoss}
* **Best** - The logging to show when the price is above target 2
* **Great** - The logging to show when the price is above target 1 but below target 2
* **Good** - The logging to show when the price is above entry but below target 1
* **Monitor** - The logging to show when the price is below entry but above stop loss
* **Bad** - The logging to show when the price is below the stop loss

* **ShowForks** - Whether or not to show Bitcoin Forked data (from settings)
* **Fork##** - The bitcoin fork.  Format {coinName, bitcoinMultiplier}

