# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Changes
05/10/2017 - Public Preview Release
05/19/2017 - Added Key definition to the metadata element in all four TrustframeworkBase.xml versions. When this Item Key is set to TRUE, the expiration dates on the token issued by B2C will be presented as JSON Numbers.  When set to False (default) they will be presented as strings.
```xml
<Item Key="SendTokenResponseBodyWithJsonNumbers">true</Item> 
```
--------------------------------------------

