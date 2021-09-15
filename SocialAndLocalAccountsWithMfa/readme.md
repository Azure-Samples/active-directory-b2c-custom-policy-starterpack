# Local and social accounts sign-up or sign-in and MFA user journey overview

Azure Active Directory B2C (Azure AD B2C) integrates directly with Azure AD Multi-Factor Authentication so that you can add a second layer of security to sign-up and sign-in experiences in your applications. For more information, see [Enable multi-factor authentication in Azure Active Directory B2C](https://docs.microsoft.com/azure/active-directory-b2c/multi-factor-authentication?pivots=b2c-custom-policy)

This article gives an overview of the **local and social accounts sign-up or sign-in with MFA** user journey custom policies. We recommend you to check out the [Local and social accounts sign-up or sign-in user journey](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/master/SocialAndLocalAccounts) before reading this article.


The _SocialAndLocalAccountsWithMfa_ starter pack relies on the [SocialAndLocalAccounts](https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/tree/master/SocialAndLocalAccounts). The following are the elements that you have to add to your policy to support MFA.

## Claim types

A claim provides a temporary storage of data during an Azure AD B2C policy execution. The [claims schema](https://docs.microsoft.com/azure/active-directory-b2c/claimsschema) is the place where you declare your claims. The following elements are used to define the claim:

```xml
<!--
<BuildingBlocks>
  <ClaimsSchema> -->
    <ClaimType Id="strongAuthenticationPhoneNumber">
      <DisplayName>Phone Number</DisplayName>
      <DataType>string</DataType>
      <Mask Type="Simple">XXX-XXX-</Mask>
      <UserHelpText>Your telephone number</UserHelpText>
    </ClaimType>

    <ClaimType Id="Verified.strongAuthenticationPhoneNumber">
      <DisplayName>Verified Phone Number</DisplayName>
      <DataType>string</DataType>
      <DefaultPartnerClaimTypes>
        <Protocol Name="OpenIdConnect" PartnerClaimType="phone_number" />
      </DefaultPartnerClaimTypes>
      <Mask Type="Simple">XXX-XXX-</Mask>
      <UserHelpText>Your office phone number that has been verified</UserHelpText>
    </ClaimType>

    <ClaimType Id="newPhoneNumberEntered">
      <DisplayName>New Phone Number Entered</DisplayName>
      <DataType>boolean</DataType>
    </ClaimType>

    <ClaimType Id="userIdForMFA">
      <DisplayName>UserId for MFA</DisplayName>
      <DataType>string</DataType>
    </ClaimType>
  <!--
  </ClaimsSchema>
</BuildingBlocks> -->
```

## Claims transformation

The _CreateUserIdForMFA_ claims transformation creates a unique identifier for the user. The identifier is used when Azure AD B2C sends and verifies the code.

```xml
<!--
<BuildingBlocks>
  <ClaimsTransformations> -->
    <ClaimsTransformation Id="CreateUserIdForMFA" TransformationMethod="FormatStringClaim">
      <InputClaims>
        <InputClaim ClaimTypeReferenceId="objectId" TransformationClaimType="inputClaim" />
      </InputClaims>
      <InputParameters>
        <InputParameter Id="stringFormat" DataType="string" Value="{0}@{RelyingPartyTenantId}" />
      </InputParameters>
      <OutputClaims>
        <OutputClaim ClaimTypeReferenceId="userIdForMFA" TransformationClaimType="outputClaim" />
      </OutputClaims>
    </ClaimsTransformation>
  <!--
  </ClaimsTransformations>
</BuildingBlocks> -->
```

### Content definitions

The following [content definition](https://docs.microsoft.com/azure/active-directory-b2c/contentdefinitions) is used to render the MFA registration and verification. 

```xml
<!--
<BuildingBlocks>
  <ContentDefinitions> -->
    <ContentDefinition Id="api.phonefactor">
      <LoadUri>~/tenant/templates/AzureBlue/multifactor-1.0.0.cshtml</LoadUri>
      <RecoveryUri>~/common/default_page_error.html</RecoveryUri>
      <DataUri>urn:com:microsoft:aad:b2c:elements:contract:multifactor:1.2.5</DataUri>
      <Metadata>
        <Item Key="DisplayName">Multi-factor authentication page</Item>
      </Metadata>
    </ContentDefinition>
  <!--
  </ContentDefinitions>
</BuildingBlocks> -->
```

## Technical profiles

The following technical profiles in used to support MFA.


|Technical profile  |Type  |Description  |Changes from the  SocialAndLocalAccounts |
|---------|---------|---------|---------|
|PhoneFactor-InputOrVerify | [Phone Factor](https://docs.microsoft.com/azure/active-directory-b2c/phone-factor-technical-profile) | Provides a user interface to interact with the user to verify, or enroll a phone number.| New |
|AAD-UserReadUsingAlternativeSecurityId | [AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) | | |
|AAD-UserWriteUsingLogonEmail |[AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) |  | Persists the phone number to the user profile. |
|AAD-UserReadUsingEmailAddress |[AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) | | Returns the phone number to the user profile.|
|AAD-UserWritePasswordUsingObjectId |[AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) | Update user's password | Persists the phone number to the user profile.|
|AAD-UserWriteProfileUsingObjectId |[AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) | Update user's profile | Persists the phone number to the user profile. |
|AAD-UserReadUsingObjectId |[AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) | Read user profile by user object ID| Returns the phone number to the user profile. |
|AAD-UserWritePhoneNumberUsingObjectId |[AzureActiveDirectory](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-technical-profile) | Persists the phone number to the user profile. | New |
|LocalAccountDiscoveryUsingEmailAddress | [SelfAsserted](https://docs.microsoft.com/azure/active-directory-b2c/self-asserted-technical-profile) | Password reset flow | Returns the phone number to the user profile. |
|LocalAccountWritePasswordUsingObjectId |[SelfAsserted](https://docs.microsoft.com/azure/active-directory-b2c/self-asserted-technical-profile) |  | Input claim |
|SM-MFA |[SSO](https://docs.microsoft.com/azure/active-directory-b2c/custom-policy-reference-sso) | MFA session manager | New |


```xml
<!-- 
<ClaimsProviders> -->
  <ClaimsProvider>
    <DisplayName>PhoneFactor</DisplayName>
    <TechnicalProfiles>
      <TechnicalProfile Id="PhoneFactor-InputOrVerify">
        <DisplayName>PhoneFactor</DisplayName>
        <Protocol Name="Proprietary" Handler="Web.TPEngine.Providers.PhoneFactorProtocolProvider, Web.TPEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
        <Metadata>
          <Item Key="ContentDefinitionReferenceId">api.phonefactor</Item>
          <Item Key="ManualPhoneNumberEntryAllowed">true</Item>
        </Metadata>
        <CryptographicKeys>
          <Key Id="issuer_secret" StorageReferenceId="B2C_1A_TokenSigningKeyContainer" />
        </CryptographicKeys>
        <InputClaimsTransformations>
          <InputClaimsTransformation ReferenceId="CreateUserIdForMFA" />
        </InputClaimsTransformations>
        <InputClaims>
          <InputClaim ClaimTypeReferenceId="userIdForMFA" PartnerClaimType="UserId" />
          <InputClaim ClaimTypeReferenceId="strongAuthenticationPhoneNumber" />
        </InputClaims>
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber" PartnerClaimType="Verified.OfficePhone" />
          <OutputClaim ClaimTypeReferenceId="newPhoneNumberEntered" PartnerClaimType="newPhoneNumberEntered" />
        </OutputClaims>
        <UseTechnicalProfileForSessionManagement ReferenceId="SM-MFA" />
      </TechnicalProfile>
    </TechnicalProfiles>
  </ClaimsProvider>
  <ClaimsProvider>
    <DisplayName>Azure Active Directory</DisplayName>
    <TechnicalProfiles>
      <TechnicalProfile Id="AAD-UserReadUsingAlternativeSecurityId">
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="strongAuthenticationPhoneNumber"/>
        </OutputClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserWriteUsingLogonEmail">
        <PersistedClaims>
          <PersistedClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber" PartnerClaimType="strongAuthenticationPhoneNumber"/>
          </PersistedClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserReadUsingEmailAddress">
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="strongAuthenticationPhoneNumber"/>
        </OutputClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserWritePasswordUsingObjectId">
        <PersistedClaims>
          <PersistedClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber" PartnerClaimType="strongAuthenticationPhoneNumber"/>
        </PersistedClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserWriteProfileUsingObjectId">
        <PersistedClaims>
          <PersistedClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber" PartnerClaimType="strongAuthenticationPhoneNumber"/>
        </PersistedClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserReadUsingObjectId">
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="strongAuthenticationPhoneNumber"/>
        </OutputClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="AAD-UserWritePhoneNumberUsingObjectId">
        <Metadata>
          <Item Key="Operation">Write</Item>
          <Item Key="RaiseErrorIfClaimsPrincipalAlreadyExists">false</Item>
          <Item Key="RaiseErrorIfClaimsPrincipalDoesNotExist">true</Item>
        </Metadata>
        <IncludeInSso>false</IncludeInSso>
        <InputClaims>
          <InputClaim ClaimTypeReferenceId="objectId" Required="true"/>
        </InputClaims>
        <PersistedClaims>
          <PersistedClaim ClaimTypeReferenceId="objectId"/>
          <PersistedClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber" PartnerClaimType="strongAuthenticationPhoneNumber"/>
        </PersistedClaims>
        <IncludeTechnicalProfile ReferenceId="AAD-Common"/>
      </TechnicalProfile>
    </TechnicalProfiles>
  </ClaimsProvider>
  <ClaimsProvider>
    <DisplayName>Local Account</DisplayName>
    <TechnicalProfiles>
      <TechnicalProfile Id="LocalAccountDiscoveryUsingEmailAddress">
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="strongAuthenticationPhoneNumber"/>
        </OutputClaims>
      </TechnicalProfile>
      <TechnicalProfile Id="LocalAccountWritePasswordUsingObjectId">
        <InputClaims>
          <InputClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber"/>
        </InputClaims>
      </TechnicalProfile>
    </TechnicalProfiles>
  </ClaimsProvider>
  <ClaimsProvider>
    <DisplayName>Session Management</DisplayName>
    <TechnicalProfiles>
      <TechnicalProfile Id="SM-MFA">
        <DisplayName>Session Mananagement Provider</DisplayName>
        <Protocol Name="Proprietary" Handler="Web.TPEngine.SSO.DefaultSSOSessionProvider, Web.TPEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"/>
        <PersistedClaims>
          <PersistedClaim ClaimTypeReferenceId="Verified.strongAuthenticationPhoneNumber"/>
        </PersistedClaims>
        <OutputClaims>
          <OutputClaim ClaimTypeReferenceId="isActiveMFASession" DefaultValue="true"/>
        </OutputClaims>
      </TechnicalProfile>
    </TechnicalProfiles>
  </ClaimsProvider>
<!-- 
</ClaimsProviders> -->
```

## User journeys

The following are the required orchestration steps required for MFA. The _PhoneFactor-Verify_ registers (if the phone number claim is empty), or verifies (if the phone number is stored in the directory).  

```xml
<UserJourneys>
  <UserJourney Id="SignUpOrSignIn">
    <OrchestrationSteps>
      ...
      <OrchestrationStep Order="7" Type="ClaimsExchange">
        <Preconditions>
          <Precondition Type="ClaimsExist" ExecuteActionsIf="true">
            <Value>isActiveMFASession</Value>
            <Action>SkipThisOrchestrationStep</Action>
          </Precondition>
        </Preconditions>
        <ClaimsExchanges>
          <ClaimsExchange Id="PhoneFactor-Verify" TechnicalProfileReferenceId="PhoneFactor-InputOrVerify"/>
        </ClaimsExchanges>
      </OrchestrationStep>
      <OrchestrationStep Order="8" Type="ClaimsExchange">
        <Preconditions>
          <Precondition Type="ClaimsExist" ExecuteActionsIf="false">
            <Value>newPhoneNumberEntered</Value>
            <Action>SkipThisOrchestrationStep</Action>
          </Precondition>
        </Preconditions>
        <ClaimsExchanges>
          <ClaimsExchange Id="AADUserWriteWithObjectId" TechnicalProfileReferenceId="AAD-UserWritePhoneNumberUsingObjectId"/>
        </ClaimsExchanges>
      </OrchestrationStep>
      ...
    </OrchestrationSteps>
  </UserJourney>

  <UserJourney Id="ProfileEdit">
    <OrchestrationSteps>
      ...
      <OrchestrationStep Order="5" Type="ClaimsExchange">
        <ClaimsExchanges>
          <ClaimsExchange Id="PhoneFactor" TechnicalProfileReferenceId="PhoneFactor-InputOrVerify"/>
        </ClaimsExchanges>
      </OrchestrationStep>
      ...
    </OrchestrationSteps>
    <ClientDefinition ReferenceId="DefaultWeb"/>
  </UserJourney>

  <UserJourney Id="PasswordReset">
    <OrchestrationSteps>
      ...
      <OrchestrationStep Order="2" Type="ClaimsExchange">
        <ClaimsExchanges>
          <ClaimsExchange Id="PhoneFactor-Verify" TechnicalProfileReferenceId="PhoneFactor-InputOrVerify"/>
        </ClaimsExchanges>
      </OrchestrationStep>
      ...
    <ClientDefinition ReferenceId="DefaultWeb"/>
  </UserJourney>

</UserJourneys>
```