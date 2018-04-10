#https://github.com/nadnerb/terraform-ec2-module/blob/master/main.tf
variable "name" {}
variable "environment" { default = "default" }
variable "role_tag" { default = "default" }
variable "environment_tag" { default = "default" }
variable "costcenter_tag" { default = "default" }
variable "ami" {}
variable "iam_role" { default = "" }
variable "instance_type" {}
variable "key_name" {}
variable "key_path" {}
variable "public_ip" {}
variable "subnet" {}
variable "security_groups" {}
variable "num_nodes" {}
variable "stream_tag" {}

resource "aws_instance" "ec2" {

  instance_type = "${var.instance_type}"

  ami = "${var.ami}"
  iam_instance_profile = "${var.iam_role}"
  subnet_id = "${var.subnet}"

  associate_public_ip_address = "${var.public_ip}"

  # Our Security groups
  security_groups = ["${split(",", replace(var.security_groups, "/,\\s?$/", ""))}"]
  key_name = "${var.key_name}"

  # consul nodes in subnet
  count = "${var.num_nodes}"

  connection {
    user = "ubuntu"
    type = "ssh"
    host = "${self.private_ip}"
    # The path to your keyfile
    key_file = "${var.key_path}"
  }

  tags {
    Name = "${var.name}-${count.index+1}"
    consul = "agent-${var.environment}"
    # required for ops reporting
    Stream = "${var.stream_tag}"
    ServerRole = "${var.role_tag}"
    "Cost Center" = "${var.costcenter_tag}"
    Environment = "${var.environment_tag}"
  }

}