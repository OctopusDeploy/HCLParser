resource "azurerm_public_ip" "owncloud" {
    name = "${var.resource_name}"
  location               = "${azurerm_resource_group.owncloud.location}"
  resource_group_name    = "${azurerm_resource_group.owncloud.name}"
    public_ip_address_allocation = "Dynamic"
}

resource "azurerm_network_interface" "owncloud" {
  name                = "${var.resource_name}"
  location               = "${azurerm_resource_group.owncloud.location}"
  resource_group_name    = "${azurerm_resource_group.owncloud.name}"

  ip_configuration {
    name                          = "${var.resource_name}"
    subnet_id                     = "${azurerm_subnet.owncloud.id}"
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id = "${azurerm_public_ip.owncloud.id}"
  }
}

resource "azurerm_virtual_machine" "owncloud" {
  name                  = "${var.resource_name}"
  location               = "${azurerm_resource_group.owncloud.location}"
  resource_group_name    = "${azurerm_resource_group.owncloud.name}"
  network_interface_ids = ["${azurerm_network_interface.owncloud.id}"]
  vm_size               = "${var.server_type}"

  storage_image_reference {
    publisher = "Canonical"
    offer     = "UbuntuServer"
    sku       = "16.04-LTS"
    version   = "latest"
  }

  storage_os_disk {
    name              = "${var.resource_name}-os"
    caching           = "ReadWrite"
    create_option     = "FromImage"
    managed_disk_type = "Standard_LRS"
  }

  os_profile {
    computer_name  = "${var.server_name}"
    admin_username = "${var.server_admin}"
    admin_password = "${var.server_password}"
  }

  os_profile_linux_config {
    disable_password_authentication = false
  }
}

resource "azurerm_virtual_machine_extension" "owncloud" {
  name                 = "${var.server_name}"
  location               = "${azurerm_resource_group.owncloud.location}"
  resource_group_name    = "${azurerm_resource_group.owncloud.name}"
  virtual_machine_name = "${azurerm_virtual_machine.owncloud.name}"
  publisher            = "Microsoft.Azure.Extensions"
  type                 = "CustomScript"
  type_handler_version = "2.0"
  auto_upgrade_minor_version = true

  settings = "${data.template_file.settings.rendered}"
  protected_settings = "${data.template_file.provision.rendered}"
}
