# Local and social accounts sign-up or sign-in user journey overview

This article gives an overview of the **local and social accounts sign-up or sign-in** user journey custom policies. We recommend you to read the [Azure AD B2C custom policy overview](https://docs.microsoft.com/azure/active-directory-b2c/custom-policy-overview) before reading this article.


You will find the user journey and its orchestration steps in the TrustFrameworkBase.xml file, with the Id "SignUpOrSignIn". Each Orchestration step and its referenced technical profile will be explained in detail in the following series.

For a user to be able to Sign in and Sign Up, the following User Experience must be translated into logical steps with a custom policy.

## Logical Steps

Handling Sign In for a Local Account:

1. Display a page where the user can enter their email and password.
1. On the sign in page, display a link to sign up.
1. If the user submits their credentials (signs in), we must validate the credentials.
1. Issue an id token.

Handling Sign In/Up for a SocialAccount:

1. Display a page where the user can select to use their Facebook account.
1. When the user clicks to "Login with Facebook", the user will be redirected to Facebook.
1. When the user returns from Facebook, read the information Facebook provided.
1. Lookup the account in the Azure AD B2C directory to determine if this user has already signed in with Facebook previously.
1. Display a page where the user can modify the data, returned from Facebook about their profile if this is their first time logging in with Facebook.
1. Write the account information to Azure AD B2C if the account was not already present in the directory.
1. Issue an id token.

Handling Sign Up for a Local Account:
1. Display a page that allows users to enter their email, password, and name.
1. Verify their email with a Timed One Time Passcode sent to their email address.
1. When the user completes a sign up, we must create their account.
1. Prevent a user to sign up with an existing email address.
1. Issue an id token.

## Translating this into custom policies
  
Handling Sign In for a Local Account:

1. This requires a Self-Asserted technical profile. It must present output claims to obtain the email and password claims.
1. Use the combined sign in and sign up content definition, which provides this for us.
1. Run a Validation technical profile to validate the credentials.
1. Read any additional information from the directory user object.
1. Call a technical profile to issue a token.  

Handling Sign In/Up for a SocialAccount:

1. Display a page where the user can select to use their Facebook account.
1. When the user clicks to "Login with Facebook", the user will be redirected to Facebook.
1. Lookup the account in the Azure AD B2C directory to determine if this user has already signed in with Facebook previously.
1. Display a page where the user can modify the data, returned from Facebook about their profile if this is their first time logging in with Facebook.
1. Write the account information to Azure AD B2C if the account was not already present in the directory.
1. Issue an id token.

1. Using the combined sign in and sign up page, we must instruct Azure AD B2C that there is a new claims provider - Facebook. This will present a button on the page to "Login with Facebook" 
1. An OAuth2 technical profile must be configured to be able to redirect the user to Facebook.
1. Use an Azure Active Directory technical profile to read the directory based off of the user identifier returned from Facebook. Usually the subject claim.
1. Use a Self-Asserted technical profile, which presents the first name and last name retrieved from Facebook in editable text boxes.
1. Use an Azure Active Directory technical profile to write the account data into the Azure AD B2C directory.
1. Call a technical profile to issue a token.

Handling Sign Up for a Local Account:

1. This requires a Self-Asserted technical profile. It must present output claims to obtain the email, password, and name claims.
1. Make use of a special claim which enforced email verification.
1. Use a Validation technical profile to write the account to the directory. This Validation technical profile will be of type Azure Active Directory.
1. As part of writing the account configures the technical profile to throw an error if the account exists.
1. Read any additional information from the directory user object.
1. Call a technical profile to issue a token.


## Understand the SocialAndLocalAccounts starter pack implementation

The SocialAndLocalAccounts starter pack comes prebuilt with a lot of functionality for the various scenarios presented within the starter pack - Sign In, Sign Up, Password Reset and Profile Edit.
When reading the user journey for a social and local account sign up or sign in, a fraction of the foundational elements contained within the files are being used. The following will unpick the elements and describe in detail the operation of a single journey.

### Handling Sign In for a Local Account and Social Account

**Orchestration Step 1**: Provide functionality for a user to Sign in or Sign Up. This is achieved using a Self-Asserted technical profile and connected validation technical profile.

The XML required to generate this step is:

```xml
<OrchestrationStep Order="1" Type="CombinedSignInAndSignUp" ContentDefinitionReferenceId="api.signuporsignin">
  <ClaimsProviderSelections>
    <ClaimsProviderSelection TargetClaimsExchangeId="FacebookExchange" />
    <ClaimsProviderSelection ValidationClaimsExchangeId="LocalAccountSigninEmailExchange" />
  </ClaimsProviderSelections>
  <ClaimsExchanges>
    <ClaimsExchange Id="LocalAccountSigninEmailExchange" TechnicalProfileReferenceId="SelfAsserted-LocalAccountSignin-Email" />
  </ClaimsExchanges>
</OrchestrationStep>
```

The combined sign in and sign up page is treated specially by Azure AD B2C, since it presents a sign up link that can take the user to the sign up step.
This is achieved with the following two lines:

```xml
<OrchestrationStep Order="1" Type="CombinedSignInAndSignUp" ContentDefinitionReferenceId="api.signuporsignin">
```

Since Azure AD B2C understands that this is a Sign In page, you must specify the `ClaimsProviderSelections` element with at least one reference to a `ClaimsProviderSelection`. This `ClaimsProviderSelection` maps to the `ClaimsExchange`. In this case, there are two `ClaimsProviderSelection` elements, such that Azure AD B2C understands that there is a Local Account and Facebook option to present on the page. The Local Account `ClaimsProviderSelection` maps to the `LocalAccountSigninEmailExchange` claims exchange, which will call the `SelfAsserted-LocalAccountSignin-Email` technical profile.

The `SelfAsserted-LocalAccountSignin-Email` technical profile defines the actual page functionality, allowing the user to sign in:

```xml
<TechnicalProfile Id="SelfAsserted-LocalAccountSignin-Email">
  <DisplayName>Local Account Signin</DisplayName>
  <Protocol Name="Proprietary" Handler="Web.TPEngine.Providers.SelfAssertedAttributeProvider, Web.TPEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
  <Metadata>
    <Item Key="SignUpTarget">SignUpWithLogonEmailExchange</Item>
    <Item Key="setting.operatingMode">Email</Item>
    <Item Key="ContentDefinitionReferenceId">api.selfasserted</Item>
    <Item Key="IncludeClaimResolvingInClaimsHandling">true</Item>
  </Metadata>
  <IncludeInSso>false</IncludeInSso>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="signInName" DefaultValue="{OIDC:LoginHint}" AlwaysUseDefaultValue="true" />
  </InputClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="signInName" Required="true" />
    <OutputClaim ClaimTypeReferenceId="password" Required="true" />
    <OutputClaim ClaimTypeReferenceId="objectId" />
    <OutputClaim ClaimTypeReferenceId="authenticationSource" />
  </OutputClaims>
  <ValidationTechnicalProfiles>
    <ValidationTechnicalProfile ReferenceId="login-NonInteractive" />
  </ValidationTechnicalProfiles>
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-AAD" />
</TechnicalProfile>
```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id | Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere, in this case from the Orchestration step.|
|DisplayName|Friendly name, which can describe the function of this technical profile.|
|Protocol|The Azure AD B2C technical profile type. In this case, it is Self-Asserted, such that a page is rendered for the user to provide their inputs.|
|Metadata|For a Self-Asserted Combined Sign in and Sign up profile, we provide a SignUpTarget, which points to the Sign Up ClaimsExchange Id in a subsequent orchestrations step.|
|InputClaims|Enables the ability to pre-populate the signInName claim|
|OutputClaims| We require the user to provide their email and password, hence referenced as output claims. There are some claims here, such as objectId, that are not presented on the page since the validation technical profile satisfies this output claim.|
|ValidationTechnicalProfiles|The technical profile to launch to validate the date the user provided, in this case to validate their credentials.|
|UseTechnicalProfileForSessionManagement|TO DO|

To see all the configuration options for a Self-Asserted technical profile, find more [here](https://docs.microsoft.com/azure/active-directory-b2c/self-asserted-technical-profile).

By calling this technical profile, we now satisfy the initial logical step for sign in. When the user submits the page, the Validation technical profile will run, called `login-NonInteractive`.

```xml
  <ValidationTechnicalProfile ReferenceId="login-NonInteractive" />
```

This is a technical profile, which makes an OpenID request using the [Resource Owner Password Credential](https://tools.ietf.org/html/rfc6749#section-4.3) grant flow to validate the user's credentials at the Azure AD authorization server. Essentially this is an API-based logon, which the Azure AD B2C server will complete against the Azure AD authorization server.

```xml
<TechnicalProfile Id="login-NonInteractive">
  <DisplayName>Local Account SignIn</DisplayName>
  <Protocol Name="OpenIdConnect" />
  <Metadata>
    <Item Key="UserMessageIfClaimsPrincipalDoesNotExist">We can't seem to find your account</Item>
    <Item Key="UserMessageIfInvalidPassword">Your password is incorrect</Item>
    <Item Key="UserMessageIfOldPasswordUsed">Looks like you used an old password</Item>

    <Item Key="ProviderName">https://sts.windows.net/</Item>
    <Item Key="METADATA">https://login.microsoftonline.com/{tenant}/.well-known/openid-configuration</Item>
    <Item Key="authorization_endpoint">https://login.microsoftonline.com/{tenant}/oauth2/token</Item>
    <Item Key="response_types">id_token</Item>
    <Item Key="response_mode">query</Item>
    <Item Key="scope">email openid</Item>

    <!-- Policy Engine Clients -->
    <Item Key="UsePolicyInRedirectUri">false</Item>
    <Item Key="HttpBinding">POST</Item>
  </Metadata>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="signInName" PartnerClaimType="username" Required="true" />
    <InputClaim ClaimTypeReferenceId="password" Required="true" />
    <InputClaim ClaimTypeReferenceId="grant_type" DefaultValue="password" AlwaysUseDefaultValue="true" />
    <InputClaim ClaimTypeReferenceId="scope" DefaultValue="openid" AlwaysUseDefaultValue="true" />
    <InputClaim ClaimTypeReferenceId="nca" PartnerClaimType="nca" DefaultValue="1" />
  </InputClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="objectId" PartnerClaimType="oid" />
    <OutputClaim ClaimTypeReferenceId="tenantId" PartnerClaimType="tid" />
    <OutputClaim ClaimTypeReferenceId="givenName" PartnerClaimType="given_name" />
    <OutputClaim ClaimTypeReferenceId="surName" PartnerClaimType="family_name" />
    <OutputClaim ClaimTypeReferenceId="displayName" PartnerClaimType="name" />
    <OutputClaim ClaimTypeReferenceId="userPrincipalName" PartnerClaimType="upn" />
    <OutputClaim ClaimTypeReferenceId="authenticationSource" DefaultValue="localAccountAuthentication" />
  </OutputClaims>
</TechnicalProfile>
```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id | Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|DisplayName|Friendly name, which can describe the function of this technical profile.|
|Protocol|The Azure AD B2C technical profile type. In this case, it is OpenId, such that Azure AD B2C understands to make an OpenId request.|
|Metadata|Various configuration options with which to make a valid OpenId request. This also includes various error handling responses, such as incorrect password.|
|InputClaims|Passes the username and password into the POST body of the OpenId request.|
|OutputClaims| Maps the JWT issued by the authorization server into Azure AD B2C's claim bag. Here we obtain the objectId and authenticationSource, hence it is not shown on the Self-Asserted page explained previously.|

To see all the configuration options for an OpenId Connect technical profile, find more [here](https://docs.microsoft.com/azure/active-directory-b2c/openid-connect-technical-profile).

At this point, we have now rendered a sign in page to the user, has the option to Sign In with Facebook, or provide their email and password after which they are verified against the Directory.

**Orchestration Step 2**: Since Orchestration Step 1 provided a `ClaimsProviderSelection` for Facebook, this is satisfied in step 2 as part of a `ClaimsExchange`. Here the `ClaimsProviderSelection` for `FacebookExchange` is satisfied by referencing the `Facebook-OAUTH` technical profile, which provides the necessary means to redirect the user to Facebook for sign in.

```xml
<!-- Check if the user has selected to sign in using one of the social providers -->
<OrchestrationStep Order="2" Type="ClaimsExchange">
  <Preconditions>
    <Precondition Type="ClaimsExist" ExecuteActionsIf="true">
      <Value>objectId</Value>
      <Action>SkipThisOrchestrationStep</Action>
    </Precondition>
  </Preconditions>
  <ClaimsExchanges>
    <ClaimsExchange Id="FacebookExchange" TechnicalProfileReferenceId="Facebook-OAUTH" />
    <ClaimsExchange Id="SignUpWithLogonEmailExchange" TechnicalProfileReferenceId="LocalAccountSignUpWithLogonEmail" />
  </ClaimsExchanges>
</OrchestrationStep>
```

The `Facebook-OAUTH` technical profile is as follows in the base file:

```xml
<TechnicalProfile Id="Facebook-OAUTH">
  <DisplayName>Facebook</DisplayName>
  <Protocol Name="OAuth2" />
  <Metadata>
    <Item Key="ProviderName">facebook</Item>
    <Item Key="authorization_endpoint">https://www.facebook.com/dialog/oauth</Item>
    <Item Key="AccessTokenEndpoint">https://graph.facebook.com/oauth/access_token</Item>
    <Item Key="HttpBinding">GET</Item>
    <Item Key="UsePolicyInRedirectUri">0</Item>
    <Item Key="AccessTokenResponseFormat">json</Item>
  </Metadata>
  <CryptographicKeys>
    <Key Id="client_secret" StorageReferenceId="B2C_1A_FacebookSecret" />
  </CryptographicKeys>
  <InputClaims />
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="issuerUserId" PartnerClaimType="id" />
    <OutputClaim ClaimTypeReferenceId="givenName" PartnerClaimType="first_name" />
    <OutputClaim ClaimTypeReferenceId="surname" PartnerClaimType="last_name" />
    <OutputClaim ClaimTypeReferenceId="displayName" PartnerClaimType="name" />
    <OutputClaim ClaimTypeReferenceId="email" PartnerClaimType="email" />
    <OutputClaim ClaimTypeReferenceId="identityProvider" DefaultValue="facebook.com" AlwaysUseDefaultValue="true" />
    <OutputClaim ClaimTypeReferenceId="authenticationSource" DefaultValue="socialIdpAuthentication" AlwaysUseDefaultValue="true" />
  </OutputClaims>
  <OutputClaimsTransformations>
    <OutputClaimsTransformation ReferenceId="CreateRandomUPNUserName" />
    <OutputClaimsTransformation ReferenceId="CreateUserPrincipalName" />
    <OutputClaimsTransformation ReferenceId="CreateAlternativeSecurityId" />
  </OutputClaimsTransformations>
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-SocialLogin" />
</TechnicalProfile>
```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id | Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|DisplayName|Friendly name, which can describe the function of this technical profile.|
|Protocol|The Azure AD B2C technical profile type. In this case, it is OAuth2, such that Azure AD B2C understands to make an OAuth2 request.|
|Metadata|Various configuration options with which to make a valid OAuth2 request. Some of these options are specific to Facebook's requirements.|
|InputClaims|There is nothing to send to Facebook, only an OAuth2 request.|
|OutputClaims| Maps the JWT issued by the Facebook authorization server into Azure AD B2C's claim bag. Some claims have default values assigned, hence are not asked from the user.|
|OutputClaimsTransformations| Various claims transformations that are called to manipulate the data returned from the token sent back by Facebook before being added into the Azure AD B2C claims bag.|

And the `Facebook-OAUTH` technical profile has an augmentation in the Extensions file as follows to complete the setup. For administrators integrating Facebook login, these are the only parameters to modify, therefore they are added as augmentations into the Extension file, while the Base technical profile will be static for all environments.

```xml
<TechnicalProfile Id="Facebook-OAUTH">
  <Metadata>
    <Item Key="client_id">facebook_clientid</Item>
    <Item Key="scope">email public_profile</Item>
    <Item Key="ClaimsEndpoint">https://graph.facebook.com/me?fields=id,first_name,last_name,name,email</Item>
  </Metadata>
</TechnicalProfile>
```

Element name  |Description  |
|---------|---------|
|TechnicalProfile Id | Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere or in this case has the same name as in the Base file to augment it.|
|Metadata|Additional configuration options with which to make a valid OAuth2 request. These are specific to ones own federation with Facebook.|

Here is the breakdown of each claims transformation that is run after the Facebook authentication succeeds and the token is returned back to Azure AD B2C. This applies to all external Identity Provider integration.

These are run such that pre-requisites for creating the account in Azure AD B2C and also for reading the account on subsequent sign in's.

**CreateRandomUPNUserName** - This is required to generate a **prefix** for the userPrincipalName, which will be stored on the user account when created.    

```xml
<ClaimsTransformation Id="CreateRandomUPNUserName" TransformationMethod="CreateRandomString">
  <InputParameters>
    <InputParameter Id="randomGeneratorType" DataType="string" Value="GUID" />
  </InputParameters>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="upnUserName" TransformationClaimType="outputClaim" />
  </OutputClaims>
</ClaimsTransformation>
```

This claims transform generates a random string, which is in the format of a GUID and issues it into the claim called `upnUserName`.

**CreateUserPrincipalName** - This creates the final userPrincipalName.

```xml
<ClaimsTransformation Id="CreateUserPrincipalName" TransformationMethod="FormatStringClaim">
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="upnUserName" TransformationClaimType="inputClaim" />
  </InputClaims>
  <InputParameters>
    <InputParameter Id="stringFormat" DataType="string" Value="cpim_{0}@{RelyingPartyTenantId}" />
  </InputParameters>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="userPrincipalName" TransformationClaimType="outputClaim" />
  </OutputClaims>
</ClaimsTransformation>
```

This claims transform uses the `FormatStringClaim` method to create a string value using claims in the Azure AD B2C claim bag. The claim given to this transform is `upnUserName`, which is available from the output of the previous claims transform. Here the transform inserts the first input claim into `{0}` and Azure AD B2C knows the value of `{RelyingPartyTenantId}` already. Then end result is a fully formed userPrincipalName, which is output in the `userPrincipalName` claim: `aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb@something.onmicrosoft.com`.

**CreateAlternativeSecurityId** - This creates a user identifier similar to an objectId, which will be used to map the subject claim (sub) from the Facebook token to the Azure AD B2C user on subsequent logons. The generated identifier is output into the claim called `alternativeSecurityId`.

```xml
<ClaimsTransformation Id="CreateAlternativeSecurityId" TransformationMethod="CreateAlternativeSecurityId">
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="issuerUserId" TransformationClaimType="key" />
    <InputClaim ClaimTypeReferenceId="identityProvider" TransformationClaimType="identityProvider" />
  </InputClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="alternativeSecurityId" TransformationClaimType="alternativeSecurityId" />
  </OutputClaims>
</ClaimsTransformation>
```

After this, the Facebook login is complete, and the claims from the token received from Facebook have been transformed into useful entities for Azure AD B2C to use.

**Orchestration Step 3**: Read any additional data from the social account user object.

We need to determine if the social account has already been registered previously with this Azure AD B2C directory, or if this is their first logon via Facebook. Also we maybe storing additional data the user provided or other data on the user object, which allows your application/service to function correctly.

Therefore, we will attempt to read the user object for any desired attributes to add into the Azure AD B2C claims bag. This technical profile is configured such that it does not throw an error if an account is not found.

The following Orchestration step calls a technical profile called `AAD-UserReadUsingAlternativeSecurityId-NoError` which provides this functionality.
The ClaimsExchange Id is a unique name for this claims exchange that you can set.

```xml
<!-- For social IDP authentication, attempt to find the user account in the directory. -->
<OrchestrationStep Order="3" Type="ClaimsExchange">
  <Preconditions>
    <Precondition Type="ClaimEquals" ExecuteActionsIf="true">
      <Value>authenticationSource</Value>
      <Value>localAccountAuthentication</Value>
      <Action>SkipThisOrchestrationStep</Action>
    </Precondition>
  </Preconditions>
  <ClaimsExchanges>
    <ClaimsExchange Id="AADUserReadUsingAlternativeSecurityId" TechnicalProfileReferenceId="AAD-UserReadUsingAlternativeSecurityId-NoError" />
  </ClaimsExchanges>
</OrchestrationStep>
```

A **precondition** is used such that this step is only run if a Social Account authentication had been completed. This is achieved by checking whether the value of `authenticationSource` claim is equal to `localAccountAuthentication`. If `authenticationSource` does contain the value `localAccountAuthentication`, then this step is skipped, otherwise it is executed.

The referenced technical profile appears as follows:

```xml
<TechnicalProfile Id="AAD-UserReadUsingAlternativeSecurityId-NoError">
  <Metadata>
    <Item Key="RaiseErrorIfClaimsPrincipalDoesNotExist">false</Item>
  </Metadata>
  <IncludeTechnicalProfile ReferenceId="AAD-UserReadUsingAlternativeSecurityId" />
</TechnicalProfile>
```

This technical profile is taking the `AAD-UserReadUsingAlternativeSecurityId` technical profile and applying a modification to it. The modification here is only to prevent an error being raised if the user is not found in the directory. This will provide an indication if this is the first logon via Facebook for this user, or a subsequent logon.

The following implements the `AAD-UserReadUsingAlternativeSecurityId` technical profile.

```xml
<TechnicalProfile Id="AAD-UserReadUsingAlternativeSecurityId">
  <Metadata>
    <Item Key="Operation">Read</Item>
    <Item Key="RaiseErrorIfClaimsPrincipalDoesNotExist">true</Item>
    <Item Key="UserMessageIfClaimsPrincipalDoesNotExist">User does not exist. Please sign up before you can sign in.</Item>
  </Metadata>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="alternativeSecurityId" PartnerClaimType="alternativeSecurityId" Required="true" />
  </InputClaims>
  <OutputClaims>
    <!-- Required claims -->
    <OutputClaim ClaimTypeReferenceId="objectId" />
    <!-- Optional claims -->
    <OutputClaim ClaimTypeReferenceId="userPrincipalName" />
    <OutputClaim ClaimTypeReferenceId="displayName" />
    <OutputClaim ClaimTypeReferenceId="otherMails" />
    <OutputClaim ClaimTypeReferenceId="givenName" />
    <OutputClaim ClaimTypeReferenceId="surname" />
  </OutputClaims>
  <IncludeTechnicalProfile ReferenceId="AAD-Common" />
</TechnicalProfile>
```

This technical profile does not state a protocol, therefore is automatically of type `Azure Active Directory`, which provides the ability to read or write to the directory structure.


|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id|Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|Metadata|This is configured to read the directory. And to throw an error if the user is not found. This has been overridden by `AAD-UserReadUsingAlternativeSecurityId-NoError`.|
|InputClaims|This is attempting to find a user account with the `alternativeSecurityId` generated in the claims transform after the Facebook sign in completed. |
|OutputClaims|We are asking to read these claims from the directory. The Azure AD B2C claims referenced here have the same name as the attribute name in the directory. |
|IncludeTechnicalProfile|AAD-Common is included to provide the foundational functionality to read or write to the directory.|

At this point the Azure AD B2C claims bag will now contain an objectId for the Social Account user who signed in, or not if this user is signing in for the first time.

**Orchestration Step 4**: A Self-Asserted technical profile is used to display a page to the user to see the imported data from Facebook, and have the ability to modify it. This is only presented to a user who has logged in for the first time with Facebook.

```xml
<OrchestrationStep Order="4" Type="ClaimsExchange">
  <Preconditions>
    <Precondition Type="ClaimsExist" ExecuteActionsIf="true">
      <Value>objectId</Value>
      <Action>SkipThisOrchestrationStep</Action>
    </Precondition>
  </Preconditions>
  <ClaimsExchanges>
    <ClaimsExchange Id="SelfAsserted-Social" TechnicalProfileReferenceId="SelfAsserted-Social" />
  </ClaimsExchanges>
</OrchestrationStep>
```

This contains a **precondition, which skips this step if an objectId was found, since the presence of an objectId would mean the user has already signed in for the first time.

The technical profile `SelfAsserted-Social` is as follows:

```xml
<TechnicalProfile Id="SelfAsserted-Social">
  <DisplayName>User ID signup</DisplayName>
  <Protocol Name="Proprietary" Handler="Web.TPEngine.Providers.SelfAssertedAttributeProvider, Web.TPEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
  <Metadata>
    <Item Key="ContentDefinitionReferenceId">api.selfasserted</Item>
  </Metadata>
  <CryptographicKeys>
    <Key Id="issuer_secret" StorageReferenceId="B2C_1A_TokenSigningKeyContainer" />
  </CryptographicKeys>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="displayName" />
    <InputClaim ClaimTypeReferenceId="givenName" />
    <InputClaim ClaimTypeReferenceId="surname" />
  </InputClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="objectId" />
    <OutputClaim ClaimTypeReferenceId="newUser" />
    <OutputClaim ClaimTypeReferenceId="executed-SelfAsserted-Input" DefaultValue="true" />
    <OutputClaim ClaimTypeReferenceId="displayName" />
    <OutputClaim ClaimTypeReferenceId="givenName" />
    <OutputClaim ClaimTypeReferenceId="surname" />
  </OutputClaims>
  <ValidationTechnicalProfiles>
    <ValidationTechnicalProfile ReferenceId="AAD-UserWriteUsingAlternativeSecurityId" />
  </ValidationTechnicalProfiles>
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-SocialSignup" />
</TechnicalProfile>
```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id|Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|Metadata|Provides information about the content definition to reference - which will give the page a customized look and feel.|
|InputClaims|These claims ensure that any values retrieved in the previous steps, in this case Facebook authentication, are prefilled. Note that some of these claims may not have any value, for example, if Facebook did not provide any of these values, or if the claim did not appear in the OutputClaims section of the `Facebook-OAUTH` technical profile. In addition, if a claim is not in the InputClaims section, but it is in the OutputClaims section, then its value will not be prefilled, but the user will still be prompted for it (with an empty value). |
|OutputClaims|These are claims that will be presented to the user on the rendered page, potentially prefilled based on the inputClaims status. Those claims, which cannot be fulfilled by the user, such as objectId and newUser, are not shown on the screen as they are fulfilled by the validation technical profile being referenced.|
|ValidationTechnicalProfile|A validation technical profile is used to write the user account when the user submits the page confirming their information.|


When the user submits the page, the Validation technical profile will run, called `AAD-UserWriteUsingAlternativeSecurityId`. This is called since either the user account can be written successfully based on the information provided, or it cannot be. In this case, the user account should always get written successfully. However, this fits best as a validation technical profile in this case.

```xml
  <ValidationTechnicalProfile ReferenceId="AAD-UserWriteUsingAlternativeSecurityId" />
```

This technical profile appears as follows:

```xml
<TechnicalProfile Id="AAD-UserWriteUsingAlternativeSecurityId">
  <Metadata>
    <Item Key="Operation">Write</Item>
    <Item Key="RaiseErrorIfClaimsPrincipalAlreadyExists">true</Item>
    <Item Key="UserMessageIfClaimsPrincipalAlreadyExists">You are already registered, please press the back button and sign in instead.</Item>
  </Metadata>
  <IncludeInSso>false</IncludeInSso>
  <InputClaimsTransformations>
    <InputClaimsTransformation ReferenceId="CreateOtherMailsFromEmail" />
  </InputClaimsTransformations>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="alternativeSecurityId" PartnerClaimType="alternativeSecurityId" Required="true" />
  </InputClaims>
  <PersistedClaims>
    <!-- Required claims -->
    <PersistedClaim ClaimTypeReferenceId="alternativeSecurityId" />
    <PersistedClaim ClaimTypeReferenceId="userPrincipalName" />
    <PersistedClaim ClaimTypeReferenceId="mailNickName" DefaultValue="unknown" />
    <PersistedClaim ClaimTypeReferenceId="displayName" DefaultValue="unknown" />

    <!-- Optional claims -->
    <PersistedClaim ClaimTypeReferenceId="otherMails" />
    <PersistedClaim ClaimTypeReferenceId="givenName" />
    <PersistedClaim ClaimTypeReferenceId="surname" />
  </PersistedClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="objectId" />
    <OutputClaim ClaimTypeReferenceId="newUser" PartnerClaimType="newClaimsPrincipalCreated" />
    <OutputClaim ClaimTypeReferenceId="otherMails" />
  </OutputClaims>
  <IncludeTechnicalProfile ReferenceId="AAD-Common" />
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-AAD" />
</TechnicalProfile>
```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id|Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|Metadata|This is configured to write to the directory. And to throw an error if the user already exists with an error message.|
|InputClaimsTransformations|<!-- is this needed? -->|
|InputClaims|This is attempting to find a user account with the `alternativeSecurityId` generated in the claims transform after the Facebook sign in completed. |
|PersistedClaims|This section defines which claims are to be written when writing to an account.|
|OutputClaims|We are asking to read these claims from account, which was just written. The Azure AD B2C claims referenced here have the same name as the attribute name in the directory. |
|IncludeTechnicalProfile|AAD-Common is included to provide the foundational functionality to read or write to the directory.|

**Orchestration Step 5** - Read any additional data from the user object if it is a Local Account.

We maybe storing additional data the user provided or other data on the Local Account user object, which allows your application/service to function correctly.

Therefore, we will read the user object for any desired attributes to add into the Azure AD B2C claims bag.

The following Orchestration step calls a technical profile called `AAD-UserReadUsingObjectId` which provides this functionality.
The ClaimsExchange Id is unique name for this claims exchange that you can set.

```xml
<OrchestrationStep Order="5" Type="ClaimsExchange">
  <Preconditions>
    <Precondition Type="ClaimEquals" ExecuteActionsIf="true">
      <Value>authenticationSource</Value>
      <Value>socialIdpAuthentication</Value>
      <Action>SkipThisOrchestrationStep</Action>
    </Precondition>
  </Preconditions>
  <ClaimsExchanges>
    <ClaimsExchange Id="AADUserReadWithObjectId" TechnicalProfileReferenceId="AAD-UserReadUsingObjectId" />
  </ClaimsExchanges>
</OrchestrationStep>
```

A **precondition** is used such that this step is skipped if the value of `authenticationSource` is set to `socialIdpAuthentication`. This prevents it being run for Social Accounts, and only runs in the case of a Local Account logon.

The referenced technical profile is as follows:

```xml
<TechnicalProfile Id="AAD-UserReadUsingObjectId">
  <Metadata>
    <Item Key="Operation">Read</Item>
    <Item Key="RaiseErrorIfClaimsPrincipalDoesNotExist">true</Item>
  </Metadata>
  <IncludeInSso>false</IncludeInSso>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="objectId" Required="true" />
  </InputClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="signInNames.emailAddress" />
    <OutputClaim ClaimTypeReferenceId="displayName" />
    <OutputClaim ClaimTypeReferenceId="otherMails" />
    <OutputClaim ClaimTypeReferenceId="givenName" />
    <OutputClaim ClaimTypeReferenceId="surname" />
  </OutputClaims>
  <IncludeTechnicalProfile ReferenceId="AAD-Common" />
</TechnicalProfile>
```

This technical profile does not state a protocol, therefore is automatically of type Azure Active Directory, which provides the ability to read or write to the directory structure.


|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id|Identifier for this technical profile. It is used to find the technical profile that this orchestration step calls.|
|Metadata|This is configured to read the directory. And to throw an error if the user is not found.|
|InputClaims|This is asking to find a user account with the objectId in the Azure AD B2C claims bag. This objectId will have been received via the login-NonInteractive technical profile and output into the claims bag by the SelfAsserted-LocalAccountSignin-Email technical profile. |
|OutputClaims|We are asking to read these claims from the directory. The Azure AD B2C claims referenced here have the same name as the attribute name in the directory. |
|IncludeTechnicalProfile|AAD-Common is included to provide the foundational functionality to read or write to the directory.|

A special case must be noted for the `signInNames.emailAddress`, this references the attribute `signInNames` which is a collection of key value pairs. In this case, we are reading back the `emailAddress` key within the `signInNames` attribute.

**Orchestration Step 6**: In the case that the Orchestration step 4 was removed, there is a backup option here to write the Social Account into the directory at this point in the journey. In such a case, the objectId would not yet exist in the Azure AD B2C claims bag, therefore a **precondition** is used such that this step is executed if one is still not present.

```xml
<OrchestrationStep Order="6" Type="ClaimsExchange">
  <Preconditions>
    <Precondition Type="ClaimsExist" ExecuteActionsIf="true">
      <Value>objectId</Value>
      <Action>SkipThisOrchestrationStep</Action>
    </Precondition>
  </Preconditions>
  <ClaimsExchanges>
    <ClaimsExchange Id="AADUserWrite" TechnicalProfileReferenceId="AAD-UserWriteUsingAlternativeSecurityId" />
  </ClaimsExchanges>
</OrchestrationStep>
```

The functionality of the `AAD-UserWriteUsingAlternativeSecurityId` has already been explored earlier.

**Orchestration Step 7**:- Issue an id token.

In the majority of user journeys, the journey will end by issuing an id token back to the application. This orchestration step looks as follows:

```xml
<OrchestrationStep Order="7" Type="SendClaims" CpimIssuerTechnicalProfileReferenceId="JwtIssuer" />
```

The referenced technical profile is as follows:

```xml
<TechnicalProfile Id="JwtIssuer">
  <DisplayName>JWT Issuer</DisplayName>
  <Protocol Name="OpenIdConnect" />
  <OutputTokenFormat>JWT</OutputTokenFormat>
  <Metadata>
    <Item Key="client_id">{service:te}</Item>
    <Item Key="issuer_refresh_token_user_identity_claim_type">objectId</Item>
    <Item Key="SendTokenResponseBodyWithJsonNumbers">true</Item>
  </Metadata>
  <CryptographicKeys>
    <Key Id="issuer_secret" StorageReferenceId="B2C_1A_TokenSigningKeyContainer" />
    <Key Id="issuer_refresh_token_key" StorageReferenceId="B2C_1A_TokenEncryptionKeyContainer" />
  </CryptographicKeys>
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-jwt-issuer" />
</TechnicalProfile>
```

This step does not need configuring any further, but find out more [here](https://docs.microsoft.com/azure/active-directory-b2c/jwt-issuer-technical-profile) on available options.

### Handling Local Account Sign Up

To handle up sign, we must have one additional orchestration step, which allows the user to provide their email, new password, and name. And upon validating this information, we must write an account to the directory. the other steps are shared with the orchestration steps explained in `Handling Sign in`.

The additional orchestration step is as follows:

```xml
<OrchestrationStep Order="2" Type="ClaimsExchange">
  <Preconditions>
    <Precondition Type="ClaimsExist" ExecuteActionsIf="true">
      <Value>objectId</Value>
      <Action>SkipThisOrchestrationStep</Action>
    </Precondition>
  </Preconditions>
  <ClaimsExchanges>
    <ClaimsExchange Id="SignUpWithLogonEmailExchange" TechnicalProfileReferenceId="LocalAccountSignUpWithLogonEmail" />
  </ClaimsExchanges>
</OrchestrationStep>
```

Since orchestration steps run sequentially, we must not run this step if the user is trying to sign in, and only run if the user clicked the sign up link. This is achieved using the **Precondition**. Note, that during the sign in phase, the Azure AD B2C claims bag will have an objectId populated after `login-NonInteractive` has run. Therefore we can use the existence of this claim to skip this step as follows.

```xml
<Precondition Type="ClaimsExist" ExecuteActionsIf="true">
    <Value>objectId</Value>
    <Action>SkipThisOrchestrationStep</Action>
</Precondition>
```

When displaying the Combined Sign in and Sign up page, it was mentioned that the metadata of the `SelfAsserted-LocalAccountSignin-Email` technical profile configures an item called `SignUpTarget`. This enables the Sign Up link on the Combined Sign in and Sign up page to call the claims exchange in orchestration Step 2, which consequently executes the `LocalAccountSignUpWithLogonEmail` technical profile.

The technical profile is designed to capture the email, password and name of the user, and then write the account to the directory, as follows:

```xml
<TechnicalProfile Id="LocalAccountSignUpWithLogonEmail">
  <DisplayName>Email signup</DisplayName>
  <Protocol Name="Proprietary" Handler="Web.TPEngine.Providers.SelfAssertedAttributeProvider, Web.TPEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
  <Metadata>
    <Item Key="IpAddressClaimReferenceId">IpAddress</Item>
    <Item Key="ContentDefinitionReferenceId">api.localaccountsignup</Item>
    <Item Key="language.button_continue">Create</Item>
  </Metadata>
  <CryptographicKeys>
    <Key Id="issuer_secret" StorageReferenceId="B2C_1A_TokenSigningKeyContainer" />
  </CryptographicKeys>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="email" />
  </InputClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="objectId" />
    <OutputClaim ClaimTypeReferenceId="email" PartnerClaimType="Verified.Email" Required="true" />
    <OutputClaim ClaimTypeReferenceId="newPassword" Required="true" />
    <OutputClaim ClaimTypeReferenceId="reenterPassword" Required="true" />
    <OutputClaim ClaimTypeReferenceId="executed-SelfAsserted-Input" DefaultValue="true" />
    <OutputClaim ClaimTypeReferenceId="authenticationSource" />
    <OutputClaim ClaimTypeReferenceId="newUser" />

    <!-- Optional claims, to be collected from the user -->
    <OutputClaim ClaimTypeReferenceId="displayName" />
    <OutputClaim ClaimTypeReferenceId="givenName" />
    <OutputClaim ClaimTypeReferenceId="surName" />
  </OutputClaims>
  <ValidationTechnicalProfiles>
    <ValidationTechnicalProfile ReferenceId="AAD-UserWriteUsingLogonEmail" />
  </ValidationTechnicalProfiles>
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-AAD" />
</TechnicalProfile>

```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id|Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|Metadata|This is configured with a reference to a content definition to provide your custom look and feel to this page.|
|InputClaims|This will pre-popualte the email field if the email claim was acquired earlier in the journey. |
|OutputClaims|These are claims that will be presented to the user on the rendered page, potentially prefilled based on the inputClaims status. Those claims, which cannot be fulfilled by the user, such as objectId and newUser, are not shown on the screen as they are fulfilled by the validation technical profile being referenced.|
|ValidationTechnicalProfile|A validation technical profile is used to write the user account when the user submits the page confirming their information.|

To see all the configuration options for a Self-Asserted technical profile, find more [here](https://docs.microsoft.com/azure/active-directory-b2c/self-asserted-technical-profile).

Azure AD B2C uses a special partner claim type to enforce email verification on a claim, as seen here:

```xml
<OutputClaim ClaimTypeReferenceId="email" PartnerClaimType="Verified.Email" Required="true" />
```

Here we are forcing the email claim presented on screen to be verified. Azure AD B2C will therefore render the `Verify` button on the page against this text field, and only allow the user to continue if this field was verified by a code sent to the user's inbox. This technique can be used against any claim name presented to the user as an output claim (ClaimTypeReferenceId).

When the user submits the page, the Validation technical profile will run, called `AAD-UserWriteUsingLogonEmail`. This is called since either the user account can be written successfully based on the information provided, or it cannot be. In this case, the user account may not be able to be written if the account exists.

The `AAD-UserWriteUsingLogonEmail` is as follows:

```xml
<TechnicalProfile Id="AAD-UserWriteUsingLogonEmail">
  <Metadata>
    <Item Key="Operation">Write</Item>
    <Item Key="RaiseErrorIfClaimsPrincipalAlreadyExists">true</Item>
  </Metadata>
  <IncludeInSso>false</IncludeInSso>
  <InputClaims>
    <InputClaim ClaimTypeReferenceId="email" PartnerClaimType="signInNames.emailAddress" Required="true" />
  </InputClaims>
  <PersistedClaims>
    <!-- Required claims -->
    <PersistedClaim ClaimTypeReferenceId="email" PartnerClaimType="signInNames.emailAddress" />
    <PersistedClaim ClaimTypeReferenceId="newPassword" PartnerClaimType="password"/>
    <PersistedClaim ClaimTypeReferenceId="displayName" DefaultValue="unknown" />
    <PersistedClaim ClaimTypeReferenceId="passwordPolicies" DefaultValue="DisablePasswordExpiration" />

    <!-- Optional claims. -->
    <PersistedClaim ClaimTypeReferenceId="givenName" />
    <PersistedClaim ClaimTypeReferenceId="surname" />
  </PersistedClaims>
  <OutputClaims>
    <OutputClaim ClaimTypeReferenceId="objectId" />
    <OutputClaim ClaimTypeReferenceId="newUser" PartnerClaimType="newClaimsPrincipalCreated" />
    <OutputClaim ClaimTypeReferenceId="authenticationSource" DefaultValue="localAccountAuthentication" />
    <OutputClaim ClaimTypeReferenceId="userPrincipalName" />
    <OutputClaim ClaimTypeReferenceId="signInNames.emailAddress" />
  </OutputClaims>
  <IncludeTechnicalProfile ReferenceId="AAD-Common" />
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-AAD" />
</TechnicalProfile>
```

|Element name  |Description  |
|---------|---------|
|TechnicalProfile Id|Identifier for this technical profile. It is used to find the technical profile that is referenced elsewhere.|
|Metadata|This is configured to write to the directory. And to throw an error if the user already exists with an error message.|
|InputClaims|This is attempting to find a user account with the `email` provided as part of the sign up page - `LocalAccountSignUpWithLogonEmail` technical profile.|
|PersistedClaims|This section defines which claims are to be written to the account. In this case, it will automatically create the account with this information present.|
|OutputClaims|We are asking to read these claims from account, which was just written. The Azure AD B2C claims referenced here have the same name as the attribute name in the directory. |
|IncludeTechnicalProfile|AAD-Common is included to provide the foundational functionality to read or write to the directory.|

*Orchestration Step 7**:- Issue an id token.

In the majority of user journeys, the journey will end by issuing an id token back to the application. This orchestration step looks as follows:

```xml
<OrchestrationStep Order="7" Type="SendClaims" CpimIssuerTechnicalProfileReferenceId="JwtIssuer" />
```

The referenced technical profile is as follows:

```xml
<TechnicalProfile Id="JwtIssuer">
  <DisplayName>JWT Issuer</DisplayName>
  <Protocol Name="OpenIdConnect" />
  <OutputTokenFormat>JWT</OutputTokenFormat>
  <Metadata>
    <Item Key="client_id">{service:te}</Item>
    <Item Key="issuer_refresh_token_user_identity_claim_type">objectId</Item>
    <Item Key="SendTokenResponseBodyWithJsonNumbers">true</Item>
  </Metadata>
  <CryptographicKeys>
    <Key Id="issuer_secret" StorageReferenceId="B2C_1A_TokenSigningKeyContainer" />
    <Key Id="issuer_refresh_token_key" StorageReferenceId="B2C_1A_TokenEncryptionKeyContainer" />
  </CryptographicKeys>
  <UseTechnicalProfileForSessionManagement ReferenceId="SM-jwt-issuer" />
</TechnicalProfile>
```

This step does not need configuring any further, but find out more [here](https://docs.microsoft.com/azure/active-directory-b2c/jwt-issuer-technical-profile) on available options.

## Summary

By reducing the user experience to a set of logical steps, we have translated these to a set of Orchestration Steps within an Azure AD B2C policy. These orchestration steps then implement the functionality of each logical step by allowing the user to interact with pages and validate various information. Finally we issue an id token back to the application.
