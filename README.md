# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments. 

# Changes
05/10/2017 - Public Preview Release, Hellow

05/19/2017 - Added Key definition to the metadata element in all four TrustframeworkBase.xml versions. When this Item Key is set to TRUE, the expiration dates on the token issued by B2C will be presented as JSON Numbers.  When set to False (default) they will be presented as strings.
```xml
<Item Key="SendTokenResponseBodyWithJsonNumbers">true</Item> 
```
--------------------------------------------

# Pending Changes, will be committed once fully tested.
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
