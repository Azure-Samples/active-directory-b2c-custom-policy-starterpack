# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Change log

### 11 October 2021

With this version the starter pack now contains localization policy file `TrustFrameworkLocalization.xml`. The localization policy allows your policy to accommodate different languages to suit your customer needs. For more information, check the [PR #107](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/pull/107).

The new localization policy is located between the base and the extension policies:

|Policy  |Base policy  |Notes  |
|---------|---------|---------|
| B2C_1A_TrustFrameworkBase| | Contains most of the definitions. To help with troubleshooting and long-term maintenance of your policies, try to minimize the number of changes you make to this file. |
| B2C_1A_TrustFrameworkLocalization | B2C_1A_TrustFrameworkBase | Holds the localization strings. |
|B2C_1A_TrustFrameworkExtensions | B2C_1A_TrustFrameworkLocalization| Holds the unique configuration changes for your tenant.  |
| Relying Parties (RP) | B2C_1A_TrustFrameworkExtensions| For example: sign-up, sign-in, password reset, or profile edit.  |

### Migrate exiting policy to this version

To migrate from the older version of the starter pack to this version:

1. Download the starter pack and update the tenant name.
1. Upload the newer version of TrustFrameworkBase.xml file.
1. Upload the new TrustFrameworkLocalization.xml file.
1. Update your **existing** TrustFrameworkExtension.xml with the new base policy `B2C_1A_TrustFrameworkLocalization`. The following XML snippet demonstrates the base policy  **before** the change:
    
    ```xml
    <!-- file: TrustFrameworkExtensions.xml -->
    <BasePolicy>
      <TenantId>yourtenant.onmicrosoft.com</TenantId>
      <PolicyId>B2C_1A_TrustFrameworkBase</PolicyId>
    </BasePolicy>
    ```
    
    The following XML snippet demonstrates the base policy  **after** the change:

    ```xml
    <!-- file: TrustFrameworkExtensions.xml -->
    <BasePolicy>
      <TenantId>yourtenant.onmicrosoft.com</TenantId>
      <PolicyId>B2C_1A_TrustFrameworkLocalization</PolicyId>
    </BasePolicy>
    ```

1. Upload the TrustFrameworkExtension.xml policy.

### 15 September 2021

[Update](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/commit/6932a0af299950139da68faac103079406847b4a#diff-6cc2ef5ed426acc5056d6bd1b912ae4cbdeb3a00769252d35d50fb8d821d6342) to the content definition page version. With the new version the starter pack uses the page contract. For more information, see [Migrating to page layout](https://docs.microsoft.com/azure/active-directory-b2c/contentdefinitions#migrating-to-page-layout).

### 20 July 2019

Updated policies to use the new Ocean Blue template

### 29 January 2019

A collection of bugfixes, improvements to code, and additional feature support is included in this starterpack.  It is not necessary or encouraged for developers to change policies currently in production or in testing.  We do encourage the use of these new versions for all new projects.

### 10 May 2017

Public Preview Release

### 5 May 2017

Added Key definition to the metadata element in all four TrustframeworkBase.xml versions. When this Item Key is set to TRUE, the expiration dates on the token issued by B2C will be presented as JSON Numbers.  When set to False (default) they will be presented as strings.

```xml
<Item Key="SendTokenResponseBodyWithJsonNumbers">true</Item> 
```

--------------------------------------------

## Important notes

The following Change is incorporated into the latest version of starterpack (01/29/2019) - It remains here for historical purposes.
06/26/2017 - Correction to SocialAndLocalAccountswMFA in TrustFrameworkBase.xml file.

A change to fix a data loss issue related to SSO, the profile edit policy, and MFA. This issue was due to the MFA SSO technical profile not outputting the below claim in the same format that the regular MFA provider does

```XML
<TechnicalProfile Id="SM-MFA">
  <DisplayName>Session Mananagement Provider</DisplayName>
  <Protocol Name="Proprietary" Handler="Web.TPEngine.SSO.DefaultSSOSessionProvider, Web.TPEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
  <PersistedClaims>
***OLD:  <PersistedClaim ClaimTypeReferenceId="strongAuthenticationPhoneNumber" />
***CORRECTED:  <PersistedClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber" />
    <PersistedClaim ClaimTypeReferenceId="executed-PhoneFactor-Input" />
  </PersistedClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="isActiveMFASession" DefaultValue="true" />
  </OutputClaims>
</TechnicalProfile>
```
