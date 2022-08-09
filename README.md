# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Change log

### 09 August 2022

With this version the starter pack now contains a Refresh Token user journey. This journey will be executed any time an application [refreshes a token](https://docs.microsoft.com/azure/active-directory-b2c/access-tokens#request-a-token). It will check the user still exists and is enabled in the Azure AD B2C directory. It also checks that the refresh token is not expired. It compiles any claims that are not persisted in the user profile, including claims from Identity Provider's and REST API calls. A new set of refreshed tokens is then issued.

This fix allows for refresh token to be revoked from users and prevents directory deleted users from getting continued access.Change affects all starterpack samples.

|Policy  |Notes  |
|-------|-------|
| B2C_1A_TrustFrameworkBase | Added Refresh Token claims, Refresh Token ClaimsTransformations, Refresh Token Technical Profiles and Refresh Token User Journey |
| B2C_1A_SignUpOrSignIn | Added Refresh Token Endpoint to Relying Party |

### Migrate existing policy to this version

Your custom policy can invoke a custom refresh token journey. Add the following user journey to your *TrustFrameworkExtensions.xml* file to get started.

1. Open the extensions file of your policy. For example, `SocialAndLocalAccounts/TrustFrameworkExtensions.xml`.
1. Locate the [UserJourneys](userjourneys.md) element. If the element doesn't exist, add it.
1. Add the following **UserJourney** to the **UserJourneys** element.


```xml
<!--
<UserJourneys>-->
  <UserJourney Id="RedeemRefreshToken">
    <PreserveOriginalAssertion>false</PreserveOriginalAssertion>
    <OrchestrationSteps>
      <OrchestrationStep Order="1" Type="ClaimsExchange">
        <ClaimsExchanges>
          <ClaimsExchange Id="RefreshTokenSetupExchange" TechnicalProfileReferenceId="RefreshTokenReadAndSetup" />
        </ClaimsExchanges>
      </OrchestrationStep>
      <OrchestrationStep Order="2" Type="ClaimsExchange">
        <ClaimsExchanges>
          <ClaimsExchange Id="CheckRefreshTokenDateFromAadExchange" TechnicalProfileReferenceId="AAD-UserReadUsingObjectId-CheckRefreshTokenDate" />
        </ClaimsExchanges>
      </OrchestrationStep>
      <OrchestrationStep Order="3" Type="SendClaims" CpimIssuerTechnicalProfileReferenceId="JwtIssuer" />
    </OrchestrationSteps>
  </UserJourney>
<!--
</UserJourneys>-->
```

This user journey will validate that the refresh token has not been revoked. You can revoke refresh tokens in Azure AD B2C following the Microsoft Graph API [Revoke sign in sessions](/graph/api/user-revokesigninsessions) guidance.

You can add additional steps into this journey to call any other technical profiles, such as to your REST API technical profiles or Azure AD read/write technical profiles.

#### Configure the relying party policy

The relying party file must be configured to point to your custom refresh token journey. This allows Azure AD B2C to reference your refresh token journey when your app makes a refresh token request. 

Add an [Endpoint](relyingparty.md#endpoints) with `Id` set to **token** and provide a `UserJourneyReferenceId` referencing the **UserJourney Id** from the prior section. Merge the following XML snippet into your *SignUpOrSignin.xml* file.

```xml
<RelyingParty> 
  <DefaultUserJourney ReferenceId="SignUpOrSignIn" /> 
    <Endpoints> 
      <Endpoint Id="Token" UserJourneyReferenceId="RedeemRefreshToken" /> 
    </Endpoints>
    ...    
</RelyingParty> 
```

Repeat this for all Relying party files your application may invoke, such as **ProfileEdit.xml** and **PasswordReset.xml**.

#### Configure refresh token revocation evaluation

The custom refresh token journey can be used to evaluate whether the current refresh token being presented has been revoked. To implement this logic, Azure AD B2C must compare the `refreshTokenIssuedOnDateTime` and the `refreshTokensValidFromDateTime`. Create the claims schema definitions as shown in the below XML snippet in your *TrustFrameworkExtensions.xml*.

1. Open the extensions file of your policy. For example, `SocialAndLocalAccounts/TrustFrameworkExtensions.xml`.
1. Locate the [BuildingBlocks](buildingblocks.md) element. If the element doesn't exist, add it.
1. Locate the [ClaimsSchema](claimsschema.md) element. If the element doesn't exist, add it.
1. Add the following claims to the **ClaimsSchema** element.


```xml
<!--
<BuildingBlocks>
  <ClaimsSchema> -->
    <ClaimType Id="refreshTokenIssuedOnDateTime">
      <DisplayName>refreshTokenIssuedOnDateTime</DisplayName>
      <DataType>string</DataType>
      <AdminHelpText>Used to determine if the user should be permitted to reauthenticate silently via their existing refresh token.</AdminHelpText>
      <UserHelpText>Used to determine if the user should be permitted to reauthenticate silently via their existing refresh token.</UserHelpText>
    </ClaimType>
    <ClaimType Id="refreshTokensValidFromDateTime">
      <DisplayName>refreshTokensValidFromDateTime</DisplayName>
      <DataType>string</DataType>
      <AdminHelpText>Used to determine if the user should be permitted to reauthenticate silently via their existing refresh token.</AdminHelpText>
      <UserHelpText>Used to determine if the user should be permitted to reauthenticate silently via their existing refresh token.</UserHelpText>
    </ClaimType>
  <!--
  </ClaimsSchema>
</BuildingBlocks> -->
```

To check whether the refresh token has been revoked, the `refreshTokenIssuedOnDateTime` and the `refreshTokensValidFromDateTime` must be compared. Add the following [`AssertDateTimeIsGreaterThan`](date-transformations.md) **ClaimsTransformation** to your *TrustFrameworkExtensions.xml*.

1. Open the extensions file of your policy. For example, `SocialAndLocalAccounts/TrustFrameworkExtensions.xml`.
1.	Locate the [BuildingBlocks](buildingblocks.md) element. If the element doesn't exist, add it.
1.	Locate the [ClaimsTransformations](claimstransformations.md) element. If the element doesn't exist, add it.
1.	Add the following **ClaimsTransformation** to the **ClaimsTransformations** element.

```xml
<!--
<BuildingBlocks>
  <ClaimsTransformations> -->
    <ClaimsTransformation Id="AssertRefreshTokenIssuedLaterThanValidFromDate" TransformationMethod="AssertDateTimeIsGreaterThan">
      <InputClaims>
        <InputClaim ClaimTypeReferenceId="refreshTokenIssuedOnDateTime" TransformationClaimType="leftOperand" />
        <InputClaim ClaimTypeReferenceId="refreshTokensValidFromDateTime" TransformationClaimType="rightOperand" />
      </InputClaims>
      <InputParameters>
        <InputParameter Id="AssertIfEqualTo" DataType="boolean" Value="false" />
        <InputParameter Id="AssertIfRightOperandIsNotPresent" DataType="boolean" Value="true" />
        <InputParameter Id="TreatAsEqualIfWithinMillseconds" DataType="int" Value="300000" />
      </InputParameters>
    </ClaimsTransformation>
  <!--
  </ClaimsTransformations>
</BuildingBlocks> -->
```

To invoke the process to evaluate whether the refresh token has been revoked, add the following technical profile to your *TrustFrameworkExtensions.xml*.

1. Open the extensions file of your policy. For example, `SocialAndLocalAccounts/TrustFrameworkExtensions.xml`.
1.	Locate the [ClaimsProviders](claimsproviders.md) element. If the element doesn't exist, add it.
1.	Add the following **ClaimsProvider** to the **ClaimsProviders** element.
1.  Add extra claims collected from previous REST API's and Federated IDP's that have not been persisted in the directory as **OutputClaims** under the **RefreshTokenReadAndSetup** technical profile

```xml
<!--
<ClaimsProviders> -->
  <ClaimsProvider>
    <DisplayName>Refresh token journey</DisplayName>
    <TechnicalProfiles>
      <TechnicalProfile Id="RefreshTokenReadAndSetup">
        <DisplayName>Trustframework Policy Engine Refresh Token Setup Technical Profile</DisplayName>
        <Protocol Name="None" />
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="objectId" />
          <OutputClaim ClaimTypeReferenceId="refreshTokenIssuedOnDateTime" />
              <!--additional claims from REST API or Federated IDP-->
            <OutputClaim ClaimTypeReferenceId="ExtraClaim1" />
            <OutputClaim ClaimTypeReferenceId="ExtraClaim2" />
        </OutputClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserReadUsingObjectId-CheckRefreshTokenDate">
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="refreshTokensValidFromDateTime" />
        </OutputClaims>
        <OutputClaimsTransformations>
          <OutputClaimsTransformation ReferenceId="AssertRefreshTokenIssuedLaterThanValidFromDate" />
        </OutputClaimsTransformations>
        <IncludeTechnicalProfile ReferenceId="AAD-UserReadUsingObjectId" />
      </TechnicalProfile>
    </TechnicalProfiles>
  </ClaimsProvider>
<!--
</ClaimsProviders> -->
```

#### Upload the policies

1. Select the **Identity Experience Framework** menu item in your B2C tenant in the Azure portal.
1. Select **Upload custom policy**
1. Select Overwrite the custom policy if it already exists
1. In this order, upload the policy files:
    1. *TrustFrameworkExtensions.xml*
    1. *SignUpOrSignin.xml*

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
