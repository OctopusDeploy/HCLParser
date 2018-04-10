variable "aws_vpc_id" {
  default = "vpc-68c53211" # Management VPC
}

variable "aws_subnet_id" {
  default = "subnet-fc2777b4" # Public A
}

variable "private_key_name" {
  default = "openvpn-uswe2"
}

variable "private_key_path" {
  default = "~/.ssh/openvpn-uswe2.pem"
}

variable "instance_type" {
  default = "t2.small"
}

variable "instance_ami" {
  default = "ami-184d5e61" # http://aws.amazon.com/marketplace/pp/B072YZPM2M?ref=cns_srchrow
}

variable "r53_zone_id" {
  default = "Z39JQKR3Q6A8PS" # cloud.octopus.com
}

variable "r53_name" {
  default = "vpn-uswe2"
}

variable "openvpn_admin_user" {
  default = "openvpn"
}

variable "openvpn_admin_pw" {
  description = "OpenVPN Password:"
}

variable "openvpn_public_hostname" {
  default = "vpn-uswe2.cloud.octopus.com"
}

variable "lets_encrypt_email" {
  default = "devops@octopus.com"
}

variable "your_public_ip_address" {
  description = "What is your public IP address? (https://www.google.com.au/search?q=what+is+my+ip)"
}
