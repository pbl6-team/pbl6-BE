## Migration
`dotnet ef database update -s ./API/API.csproj  -o ./Infrastructure/Infrastructure.csproj `

## google auth
- url: https://accounts.google.com/o/oauth2/v2/authSW
- scope: profile+email
- include_granted_scopes :true
- response_type: code
- client_id: 945996023510-0j32cbj68e6ftd38sampakldkhok571m.apps.googleusercontent.com
- redirect_uri: {baseurl}/signin-google