# scanForItemsFix

This mod fixes the issue where the terminal outputs [scanForItems] instead of the scan results due to certain scrap values having a higher minimum value than the maximum value when the game tries to calculate the scrap value of an item.

## The error
An error occured while post processing terminal text: System.ArgumentOutOfRangeException: 'minValue' cannot be greater than maxValue.

## The fix

The fix is to simply check if the minimum value is greater than the maximum value and if so, set the minimum value to the maximum value.