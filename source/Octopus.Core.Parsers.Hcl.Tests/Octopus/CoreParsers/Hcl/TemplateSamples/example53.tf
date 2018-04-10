# Variable Definition
variable "node_count" {default = 1} # Define the number of instances

# Configure the VMware vSphere Provider. ENV Variables set for Username and Passwd.
provider "vsphere" {
 vsphere_server = "vcenter.server"
}

# Define the VM resource
resource "vsphere_virtual_machine" "example" {
 name   = "node-${format("%02d", count.index+1)}"
 folder = "vm_folder"
 vcpu   = 2
 memory = 2048
 datacenter = "datacenter"
 cluster = "vsphereCluster"

# Define the Networking settings for the VM
 network_interface {
   label = "VM Network"
   ipv4_gateway = "10.1.1.1"
   ipv4_address = "10.1.1.100"
   ipv4_prefix_length = "24"
 }

# Define Domain and DNS
 domain = "domain.com"
 dns_servers = ["my_consul1", "consul2", "consul3", "8.8.8.8"]

# Define the Disks and resources. The first disk should include the template.
 disk {
   template = "my-centos7-template"
   datastore = "vsanDatastore"
   type ="thin"
 }

 disk {
   size = "5"
   datastore = "vsanDatastore"
   type ="thin"
 }

# Define Time Zone
 time_zone = "America/New_York"

# Loop for Count
 count = "${var.node_count}"
}
