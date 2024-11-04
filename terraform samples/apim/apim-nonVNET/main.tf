# Configure the Azure provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0.2"
    }
  }

  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {
  }
}

resource "azurerm_resource_group" "rg" {
  name = "rg-dev-apimDemo"
  location = "Central India"
}

resource "azurerm_api_management" "apim" {
    name = "Test"
    location = azurerm_resource_group.rg.location
    resource_group_name = azurerm_resource_group.rg.name
    publisher_name = "karthikeyan Ramasamy"
    publisher_email = "sample@abc.com"
    sku_name = "Developer_1"
}