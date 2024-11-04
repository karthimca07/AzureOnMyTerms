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
  name ="rg-ServiceBusTopicsWithTerraform"
  location = "Central India"
}

resource "azurerm_servicebus_namespace" "sb-nm" {
  name = "ServiceBusTopicsWithTerraform"
  resource_group_name = azurerm_resource_group.rg.name
  location = azurerm_resource_group.rg.location
  sku = "Standard"
}

resource "azurerm_servicebus_topic" "sb-topic" {
  name = "sb-topic-demo"
  namespace_id = azurerm_servicebus_namespace.sb-nm.id
}

resource "azurerm_servicebus_subscription" "sb-topic-subscription-1" {
  name = "sb-topic_subscription-demo-1"
  topic_id = azurerm_servicebus_topic.sb-topic.id
  max_delivery_count = 5
}

resource "azurerm_servicebus_subscription" "sb-topic-subscription-2" {
  name = "sb-topic_subscription-demo-2"
  topic_id = azurerm_servicebus_topic.sb-topic.id
  max_delivery_count = 5
}

resource "azurerm_servicebus_subscription_rule" "sb-topic-subscription-rule-1" {
  name = "sb-topic-subscription-rule-demo-1"
  subscription_id = azurerm_servicebus_subscription.sb-topic-subscription-1.id
  filter_type = "SqlFilter"
  sql_filter = "sub1=sub1"
}


//default filter | True to False | False will be used when you want to stop delivering messages for temporarily for a subscription
resource "azurerm_servicebus_subscription_rule" "sb-topic-subscription-rule-2" {
  name = "sb-topic-subscription-rule-demo-1"
  subscription_id = azurerm_servicebus_subscription.sb-topic-subscription-2.id
  filter_type = "SqlFilter"
  sql_filter = "sub2=sub2"
}

//Subcription for correaltion filter demo
resource "azurerm_servicebus_subscription" "sb-topic-subscription-3" {
  name = "sb-topic_subscription-demo-3"
  topic_id = azurerm_servicebus_topic.sb-topic.id
  max_delivery_count = 5
}

//Subcription rule with correaltion filter
resource "azurerm_servicebus_subscription_rule" "sb-topic-subscription-rule-3" {
  name = "sb-topic-subscription-rule-demo-3"
  subscription_id = azurerm_servicebus_subscription.sb-topic-subscription-3.id
  filter_type = "CorrelationFilter"
  correlation_filter {
    label = "Demo"
    properties = {
      customprop1 = "customvalue1"
    }
  }
}

//Subcription for action filter demo
resource "azurerm_servicebus_subscription" "sb-topic-subscription-4" {
  name = "sb-topic_subscription-demo-4"
  topic_id = azurerm_servicebus_topic.sb-topic.id
  max_delivery_count = 5
}

//Subcription rule with action filter
resource "azurerm_servicebus_subscription_rule" "sb-topic-subscription-rule-4" {
  name = "sb-topic-subscription-rule-demo-4"
  subscription_id = azurerm_servicebus_subscription.sb-topic-subscription-4.id
  filter_type = "SqlFilter"
  sql_filter = "Type=ActionFilterDemo"
  action = "SET NewProp=ActionFilterInAction" 
 }

//Subcription for composite filter
resource "azurerm_servicebus_subscription" "sb-topic-subscription-5" {
  name = "sb-topic_subscription-demo-5"
  topic_id = azurerm_servicebus_topic.sb-topic.id
  max_delivery_count = 5
}

//Subcription rule with composite filter
resource "azurerm_servicebus_subscription_rule" "sb-topic-subscription-rule-5" {
  name = "sb-topic-subscription-rule-demo-5"
  subscription_id = azurerm_servicebus_subscription.sb-topic-subscription-5.id
  filter_type = "SqlFilter"
  sql_filter = "Type=CompositeFilterDemo AND CustomProp=CompositeFilterDemo"
}

