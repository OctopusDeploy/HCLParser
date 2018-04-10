variable "region" {}

variable "atlas_environment" {}
variable "atlas_username" {}
variable "atlas_token" {}
variable "name" {}
variable "cert_name" {}
variable "key_name" {}

variable "iam_admins" {}

variable "aws_consul_latest_name" {}
variable "aws_consul_pinned_name" {}
variable "aws_consul_pinned_version" {}
variable "aws_vault_latest_name" {}
variable "aws_vault_pinned_name" {}
variable "aws_vault_pinned_version" {}
variable "aws_rabbitmq_latest_name" {}
variable "aws_rabbitmq_pinned_name" {}
variable "aws_rabbitmq_pinned_version" {}
variable "aws_web_latest_name" {}
variable "aws_web_pinned_name" {}
variable "aws_web_pinned_version" {}

variable "vpc_cidr" {}
variable "public_subnets" {}
variable "private_subnets" {}
variable "ephemeral_subnets" {}
variable "azs" {}

variable "bastion_instance_type" {}
variable "nat_instance_type" {}

variable "openvpn_instance_type" {}
variable "openvpn_ami" {}
variable "openvpn_admin_user" {}
variable "openvpn_admin_pw" {}
variable "openvpn_cidr" {}

variable "domain" {}

variable "db_name" {}
variable "db_username" {}
variable "db_password" {}
variable "db_engine" {}
variable "db_engine_version" {}
variable "db_port" {}

variable "db_az" {}
variable "db_multi_az" {}
variable "db_instance_type" {}
variable "db_storage_gbs" {}
variable "db_iops" {}
variable "db_storage_type" {}
variable "db_apply_immediately" {}
variable "db_publicly_accessible" {}
variable "db_storage_encrypted" {}
variable "db_maintenance_window" {}
variable "db_backup_retention_period" {}
variable "db_backup_window" {}

variable "redis_instance_type" {}
variable "redis_port" {}
variable "redis_initial_cached_nodes" {}
variable "redis_apply_immediately" {}
variable "redis_maintenance_window" {}

variable "consul_ips" {}
variable "consul_instance_type" {}

variable "vault_count" {}
variable "vault_instance_type" {}

variable "rabbitmq_count" {}
variable "rabbitmq_instance_type" {}
# variable "rabbitmq_blue_nodes" {}
# variable "rabbitmq_green_nodes" {}
variable "rabbitmq_username" {}
variable "rabbitmq_password" {}
variable "rabbitmq_vhost" {}

variable "web_instance_type" {}
variable "web_blue_nodes" {}
variable "web_green_nodes" {}

provider "aws" {
  region = "${var.region}"
}

atlas {
  name = "${var.atlas_username}/${var.atlas_environment}"
}

module "certs" {
  source = "../../certs"

  name = "${var.cert_name}"
}

module "keys" {
  source = "../../keys"

  name = "${var.key_name}"
}

module "scripts" {
  source = "../../scripts"
}

module "access" {
  source = "../../aws/access"

  name       = "${var.atlas_environment}"
  iam_admins = "${var.iam_admins}"
  pub_path   = "${module.keys.pub_path}"
}

module "aws_network" {
  source = "../../aws/network"

  name     = "${var.name}"
  vpc_cidr = "${var.vpc_cidr}"
  azs      = "${var.azs}"
  region   = "${var.region}"
  key_name = "${module.access.key_name}"
  key_path = "${module.keys.pem_path}"

  public_subnets    = "${var.public_subnets}"
  private_subnets   = "${var.private_subnets}"
  ephemeral_subnets = "${var.ephemeral_subnets}"

  bastion_instance_type = "${var.bastion_instance_type}"
  nat_instance_type     = "${var.nat_instance_type}"
  openvpn_instance_type = "${var.openvpn_instance_type}"

  openvpn_ami        = "${var.openvpn_ami}"
  openvpn_admin_user = "${var.openvpn_admin_user}"
  openvpn_admin_pw   = "${var.openvpn_admin_pw}"
  openvpn_dns_ips    = "${var.consul_ips}"
  openvpn_cidr       = "${var.openvpn_cidr}"
  openvpn_ssl_crt    = "${module.certs.crt_path}"
  openvpn_ssl_key    = "${module.certs.key_path}"
}

module "aws_artifacts_consul" {
  source = "../../aws/artifact"

  atlas_username = "${var.atlas_username}"
  latest_name    = "${var.aws_consul_latest_name}"
  pinned_name    = "${var.aws_consul_pinned_name}"
  pinned_version = "${var.aws_consul_pinned_version}"
}

module "aws_artifacts_vault" {
  source = "../../aws/artifact"

  atlas_username = "${var.atlas_username}"
  latest_name    = "${var.aws_vault_latest_name}"
  pinned_name    = "${var.aws_vault_pinned_name}"
  pinned_version = "${var.aws_vault_pinned_version}"
}

module "aws_artifacts_rabbitmq" {
  source = "../../aws/artifact"

  atlas_username = "${var.atlas_username}"
  latest_name    = "${var.aws_rabbitmq_latest_name}"
  pinned_name    = "${var.aws_rabbitmq_pinned_name}"
  pinned_version = "${var.aws_rabbitmq_pinned_version}"
}

module "aws_data" {
  source = "../../aws/data"

  name     = "${var.name}"
  azs      = "${var.azs}"
  key_name = "${module.access.key_name}"
  key_path = "${module.keys.pem_path}"
  vpc_id   = "${module.aws_network.vpc_id}"
  vpc_cidr = "${module.aws_network.vpc_cidr}"

  private_subnet_ids   = "${module.aws_network.private_subnet_ids}"
  ephemeral_subnet_ids = "${module.aws_network.ephemeral_subnet_ids}"
  public_subnet_ids    = "${module.aws_network.public_subnet_ids}"

  consul_server_user_data = "${module.scripts.ubuntu_consul_server_user_data}"
  vault_user_data         = "${module.scripts.ubuntu_vault_user_data}"
  rabbitmq_user_data      = "${module.scripts.ubuntu_rabbitmq_user_data}"
  atlas_username          = "${var.atlas_username}"
  atlas_environment       = "${var.atlas_environment}"
  atlas_token             = "${var.atlas_token}"

  db_name           = "${var.db_name}"
  db_username       = "${var.db_username}"
  db_password       = "${var.db_password}"
  db_engine         = "${var.db_engine}"
  db_engine_version = "${var.db_engine_version}"
  db_port           = "${var.db_port}"

  db_az                      = "${var.db_az}"
  db_multi_az                = "${var.db_multi_az}"
  db_instance_type           = "${var.db_instance_type}"
  db_storage_gbs             = "${var.db_storage_gbs}"
  db_iops                    = "${var.db_iops}"
  db_storage_type            = "${var.db_storage_type}"
  db_apply_immediately       = "${var.db_apply_immediately}"
  db_publicly_accessible     = "${var.db_publicly_accessible}"
  db_storage_encrypted       = "${var.db_storage_encrypted}"
  db_maintenance_window      = "${var.db_maintenance_window}"
  db_backup_retention_period = "${var.db_backup_retention_period}"
  db_backup_window           = "${var.db_backup_window}"

  redis_instance_type        = "${var.redis_instance_type}"
  redis_port                 = "${var.redis_port}"
  redis_initial_cached_nodes = "${var.redis_initial_cached_nodes}"
  redis_apply_immediately    = "${var.redis_apply_immediately}"
  redis_maintenance_window   = "${var.redis_maintenance_window}"

  ssl_cert_name     = "${module.certs.name}"
  ssl_cert_crt      = "${module.certs.crt_path}"
  ssl_cert_key      = "${module.certs.key_path}"
  bastion_host      = "${module.aws_network.bastion_ip}"
  bastion_user      = "${module.aws_network.bastion_user}"

  # Number of AMIs must match the count. To update, change all artifacts
  # to _pinned at the previous version, then, `terraform taint` one
  # resource at a time and update to _latest.
  consul_ips           = "${var.consul_ips}"
  consul_instance_type = "${var.consul_instance_type}"
  consul_amis          = "${module.aws_artifacts_consul.latest},${module.aws_artifacts_consul.latest},${module.aws_artifacts_consul.latest}"

  # Number of AMIs must match the count. To update, change all artifacts
  # to _pinned at the previous version, then, `terraform taint` one
  # resource at a time and update to _latest.
  vault_count         = "${var.vault_count}"
  vault_instance_type = "${var.vault_instance_type}"
  vault_amis          = "${module.aws_artifacts_vault.latest},${module.aws_artifacts_vault.latest}"

  rabbitmq_count         = "${var.rabbitmq_count}"
  rabbitmq_instance_type = "${var.rabbitmq_instance_type}"
  rabbitmq_amis          = "${module.aws_artifacts_rabbitmq.latest}"
  # rabbitmq_blue_ami      = "${module.aws_artifacts_rabbitmq.latest}"
  # rabbitmq_blue_nodes    = "${var.rabbitmq_blue_nodes}"
  # rabbitmq_green_ami     = "${module.aws_artifacts_rabbitmq.pinned}"
  # rabbitmq_green_nodes   = "${var.rabbitmq_green_nodes}"
  rabbitmq_username      = "${var.rabbitmq_username}"
  rabbitmq_password      = "${var.rabbitmq_password}"
  rabbitmq_vhost         = "${var.rabbitmq_vhost}"
}

module "aws_artifacts_web" {
  source = "../../aws/artifact"

  atlas_username = "${var.atlas_username}"
  latest_name    = "${var.aws_web_latest_name}"
  pinned_name    = "${var.aws_web_pinned_name}"
  pinned_version = "${var.aws_web_pinned_version}"
}

module "aws_compute" {
  source             = "../../aws/compute"

  name               = "${var.name}"
  vpc_cidr           = "${var.vpc_cidr}"
  azs                = "${var.azs}"
  domain             = "${var.domain}"
  vpc_id             = "${module.aws_network.vpc_id}"
  private_subnet_ids = "${module.aws_network.private_subnet_ids}"
  public_subnet_ids  = "${module.aws_network.public_subnet_ids}"
  key_name           = "${module.access.key_name}"
  key_path           = "${module.keys.pem_path}"
  ssl_cert_crt       = "${module.certs.crt_path}"
  ssl_cert_key       = "${module.certs.key_path}"

  user_data         = "${module.scripts.windows_consul_client_user_data}"
  atlas_username    = "${var.atlas_username}"
  atlas_environment = "${var.atlas_environment}"
  atlas_token       = "${var.atlas_token}"
  consul_ips        = "${module.aws_data.consul_ips}"

  db_name     = "${var.db_name}"
  db_endpoint = "${module.aws_data.db_endpoint}"
  db_username = "${module.aws_data.db_username}"
  db_password = "${module.aws_data.db_password}"

  redis_host     = "${module.aws_data.redis_host}"
  redis_port     = "${module.aws_data.redis_port}"
  redis_password = "${module.aws_data.redis_password}"

  rabbitmq_host     = "${module.aws_data.rabbitmq_host}"
  rabbitmq_port     = "${module.aws_data.rabbitmq_port}"
  rabbitmq_username = "${module.aws_data.rabbitmq_username}"
  rabbitmq_password = "${module.aws_data.rabbitmq_password}"
  rabbitmq_vhost    = "${module.aws_data.rabbitmq_vhost}"

  bastion_host     = "${module.aws_network.bastion_ip}"
  bastion_user     = "${module.aws_network.bastion_user}"
  vault_private_ip = "${element(split(",", module.aws_data.vault_private_ips), 0)}"
  vault_domain     = "vault.${var.domain}"

  web_instance_type = "${var.web_instance_type}"
  web_blue_ami      = "${module.aws_artifacts_web.latest}"
  web_blue_nodes    = "${var.web_blue_nodes}"
  web_green_ami     = "${module.aws_artifacts_web.pinned}"
  web_green_nodes   = "${var.web_green_nodes}"
}

module "aws_dns" {
  source = "../../aws/dns"

  domain         = "${var.domain}"
  web_dns_name   = "${module.aws_compute.web_dns_name}"
  web_zone_id    = "${module.aws_compute.web_zone_id}"
  vault_dns_name = "${module.aws_data.vault_dns_name}"
  vpn_ip         = "${module.aws_network.openvpn_ip}"
}

resource "null_resource" "consul_ready" {
  provisioner "remote-exec" {
    connection {
      user         = "ubuntu"
      host         = "${element(split(",", module.aws_data.consul_ips), 0)}"
      key_file     = "${module.keys.pem_path}"
      bastion_host = "${module.aws_network.bastion_ip}"
      bastion_user = "${module.aws_network.bastion_user}"
    }

    inline = [ <<COMMANDS
#!/bin/bash
set -e

# Join Consul cluster
consul join ${replace(module.aws_data.consul_ips, ",", " ")}

# Remote commands utilize Consul's KV store, wait until ready
SLEEPTIME=1
cget() { curl -sf "http://127.0.0.1:8500/v1/kv/service/consul/ready?raw"; }

# Wait for the Consul cluster to become ready
while ! cget | grep "true"; do
  if [ $SLEEPTIME -gt 24 ]; then
    echo "ERROR: CONSUL DID NOT COMPLETE SETUP! Manual intervention required."
    exit 2
  else
    echo "Blocking until Consul is ready, waiting $SLEEPTIME second(s)..."
    sleep $SLEEPTIME
    ((SLEEPTIME+=1))
  fi
done

COMMANDS
]
  }
}

resource "null_resource" "remote_commands" {
  depends_on = ["null_resource.consul_ready"]

  provisioner "remote-exec" {
    connection {
      user         = "ubuntu"
      host         = "${element(split(",", module.aws_data.consul_ips), 0)}"
      key_file     = "${module.keys.pem_path}"
      bastion_host = "${module.aws_network.bastion_ip}"
      bastion_user = "${module.aws_network.bastion_user}"
    }

    inline = [
      "${module.aws_data.remote_commands}",
      "${module.aws_compute.remote_commands}",
    ]
  }
}

output "configuration" {
  value = <<CONFIGURATION

Domain:       https://${module.aws_dns.main}
Bastion IP:   ${module.aws_network.bastion_ip}
OpenVPN IP:   ${module.aws_network.openvpn_ip}
Name Servers: ${module.aws_dns.ns1},${module.aws_dns.ns2},${module.aws_dns.ns3},${module.aws_dns.ns4}

The below IAM users have been created.
  Users: ${module.access.admin_users}
  Access Key Ids: ${module.access.admin_access_key_ids}
  Secret Access Keys: ${module.access.admin_secret_access_keys}

DNS records have been set for all ${var.name} services, please add NS records for ${var.domain} pointing to:
  ${module.aws_dns.ns1}
  ${module.aws_dns.ns2}
  ${module.aws_dns.ns3}
  ${module.aws_dns.ns4}

The environment is accessible via an OpenVPN connection:
  Server:   ${module.aws_network.openvpn_ip}
  Server:   https://${module.aws_dns.vpn}/
  Username: ${var.openvpn_admin_user}
  Password: ${var.openvpn_admin_pw}

You can administer the OpenVPN Access Server here:
  https://${module.aws_network.openvpn_ip}/admin
  https://${module.aws_dns.vpn}/admin

Once you're on the VPN, you can...

Visit the Consul UI here:
  http://${element(split(",", var.consul_ips), 0)}:8500/ui
  http://consul.service.consul:8500/ui

Administer RabbitMQ here:
  http://${module.aws_data.rabbitmq_host}:15672
  Username: ${var.rabbitmq_username}
  Password: ${var.rabbitmq_password}

CONFIGURATION
}
