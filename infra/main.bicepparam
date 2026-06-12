using './main.bicep'

param environmentName = 'prod'
param location = 'germanywestcentral'
// swaLocation must be one of: centralus, eastus2, westus2, westeurope, eastasia
// None of these are in your subscription's allowed regions — contact Azure support
// to add 'westeurope', then change this value.
param swaLocation = 'westeurope'
