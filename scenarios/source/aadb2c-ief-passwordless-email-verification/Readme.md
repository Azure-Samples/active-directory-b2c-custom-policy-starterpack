# Azure AD B2C: Password-less sign-in with email verification
Passwordless authentication is a type of authentication where user doesn't need to sign-in with their password. This is commonly used in B2C scenarios where users use your application infrequently and tend to forget their password. This sample policy demonstrates how to allow user to sign-in, simply by providing and verifying the sign-in email address using OTP code (one time password). 

[![Password-less sign-in with email verification video](media/link-to-youtube.png)](https://youtu.be/bzqMDPnV5G0)

## Solution flow
There are two sign-in flows you can use:
* Password-less sign-in with local account, local account with password, or social account. 
* Password-less sign-in with local account only. For more information, see the sign-in user journey.

> Note:  This sample policy is based on [SocialAndLocalAccounts starter pack](../../../SocialAndLocalAccounts). All changes are marked with **Demo:** comment inside the policy XML files.


