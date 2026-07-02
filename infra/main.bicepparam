using './main.bicep'

param environmentName = 'prod'
// Tenant policy (sys.regionrestriction) allows: spaincentral, uaenorth, italynorth,
// germanywestcentral, switzerlandnorth. App Service is available in all of these,
// so unlike Static Web Apps, everything can now live in a single allowed region.
param location = 'germanywestcentral'
param appServicePlanSku = 'B1'

// Matches the lowercase ghcr.io/<owner>/<repo> image the docker-build-deploy.yml
// workflow builds and pushes. App Service will retry pulling until CI pushes it.
param containerImage = 'ghcr.io/lanreadetola/study-tracker:latest'

// GitHub OAuth App / Microsoft Entra App Registration credentials for App Service
// Authentication (Easy Auth). Pass these at deploy time, e.g.:
//   az deployment group create ... \
//     --parameters githubClientId=$GITHUB_CLIENT_ID githubClientSecret=$GITHUB_CLIENT_SECRET \
//     --parameters aadClientId=$AAD_CLIENT_ID aadClientSecret=$AAD_CLIENT_SECRET
// Do not commit real values here.
param githubClientId = ''
param githubClientSecret = ''
param aadClientId = ''
param aadClientSecret = ''

// Only needed if the ghcr.io package is kept private.
param registryUsername = ''
param registryPassword = ''
