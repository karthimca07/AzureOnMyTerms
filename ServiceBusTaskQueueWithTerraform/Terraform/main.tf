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
  features {}
}


resource "azurerm_resource_group" "rg" {
  name ="rgServiceBusTaskQueueWithTerraform"
  location = "central india"
}

resource "azurerm_servicebus_namespace" "sb-nm-task" {
  name = "sb-nm-task"
  location = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku = "Standard"
}