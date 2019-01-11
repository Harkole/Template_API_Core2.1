# Template_API_Core2.1
Pre-set API in Net Core 2.1 with JWT token

Base line project designed to save time by having the common/default values for a JWT secured API in Net Core. Requires that certain values are set in the appsettings.json file as follows:

- Audience(s): A single string value or comma delimited string value of acceptable audiences. No whitespaces
`"audience1,audience2,audience3`
or a single audience
 `"audience1"`
- ClockSkew: An amount of time (defaults to 5 mintues if not set) that the DateTime can be offset by
- Issuer: The name of the API/Company/Proudct that issued the token
- SecretKey: The encyption key, shouldn't ever be made public or shared to ensure valid tokens
