# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Changes

07/20/2019 - Updated policies to use the new Ocean Blue template

01/29/2019 - Updates to Starterpack

A collection of bugfixes, improvements to code, and additional feature support is included in this starterpack.  It is not necessary or encouraged for developers to change policies currently in production or in testing.  We do encourage the use of these new versions for all new projects.


05/10/2017 - Public Preview Release

05/19/2017 - Added Key definition to the metadata element in all four TrustframeworkBase.xml versions. When this Item Key is set to TRUE, the expiration dates on the token issued by B2C will be presented as JSON Numbers.  When set to False (default) they will be presented as strings.
```xml
<Item Key="SendTokenResponseBodyWithJsonNumbers">true</Item> 
```
--------------------------------------------

# The following Change is incorporated into the latest version of starterpack (01/29/2019) - It remains here for historical purposes.
06/26/2017 - Correction to SocialAndLocalAccountswMFA in TrustFrameworkBase.xml file.


•	A change to fix a data loss issue related to SSO, the profile edit policy, and MFA. This issue was due to the MFA SSO technical profile not outputting the below claim in the same format that the regular MFA provider does

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
