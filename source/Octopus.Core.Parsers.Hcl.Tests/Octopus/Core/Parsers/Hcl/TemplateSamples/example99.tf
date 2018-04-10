resource "azurerm_resource_group" "owncloud" {
  name     = "${var.resource_name}"
  location = "${var.resource_region}"
}
