# SSH authorized public key
resource "openstack_compute_keypair_v2" "ssh_keypair" {
  count      = terraform.workspace == "default" ? 1 : 0
  name       = local.keypair_pubkey_name
  public_key = file("../ssh_wai_key.pub")
}

# Main network router
resource "openstack_networking_router_v2" "main_router" {
  count               = terraform.workspace == "default" ? 1 : 0
  description         = "Infrastructure main router"
  name                = local.main_router_name
  external_network_id = var.spccloud_public_network_id
}

# Kubernetes nodes
module "kubernetes" {
  source                     = "./modules/kubernetes"
  enabled                    = terraform.workspace == "production" && var.environment == "production"
  project_name_prefix        = var.project_name_prefix
  main_router_name           = local.main_router_name
  public_dns_ips             = var.public_dns_ips
  ssh_keypair_name           = local.keypair_pubkey_name
  ssh_user                   = var.ssh_user
  k8s_network_cidr           = var.k8s_network_cidr
  k8s_master_floatingips     = var.k8s_master_floatingips
  k8s_master_instance        = var.k8s_master_instance
  k8s_master_instance_groups = var.k8s_master_instance_groups
  k8s_master_sec_rules       = var.k8s_master_sec_rules
  k8s_worker_floatingips     = var.k8s_worker_floatingips
  k8s_worker_instance        = var.k8s_worker_instance
  k8s_worker_instance_groups = var.k8s_worker_instance_groups
  k8s_worker_sec_rules       = var.k8s_worker_sec_rules
  k8s_metallb_address_pairs  = var.k8s_metallb_address_pairs
  k8s_metallb_port_sec_rules = var.k8s_metallb_port_sec_rules
}

# Elastic nodes
module "elastic" {
  source                  = "./modules/generic-cluster"
  enabled                 = terraform.workspace == "production" && var.environment == "production"
  environment_short       = var.environment_short
  project_name_prefix     = var.project_name_prefix
  main_router_name        = local.main_router_name
  public_dns_ips          = var.public_dns_ips
  ssh_keypair_name        = local.keypair_pubkey_name
  ssh_user                = var.ssh_user
  cluster_slug            = var.elastic_slug
  cluster_network_cidr    = var.elastic_network_cidr
  cluster_instance        = var.elastic_instance
  cluster_instance_groups = var.elastic_instance_groups
  cluster_sec_rules       = var.elastic_sec_rules
}

# Glusterfs nodes
module "gluster" {
  source                  = "./modules/generic-cluster"
  enabled                 = terraform.workspace == "production" && var.environment == "production"
  environment_short       = var.environment_short
  project_name_prefix     = var.project_name_prefix
  main_router_name        = local.main_router_name
  public_dns_ips          = var.public_dns_ips
  ssh_keypair_name        = local.keypair_pubkey_name
  ssh_user                = var.ssh_user
  cluster_slug            = var.gluster_slug
  cluster_network_cidr    = var.gluster_network_cidr
  cluster_instance        = var.gluster_instance
  cluster_instance_groups = var.gluster_instance_groups
  cluster_sec_rules       = var.gluster_sec_rules
}

# Galera production nodes
module "galera_production" {
  source                  = "./modules/generic-cluster"
  enabled                 = terraform.workspace == "production" && var.environment == "production"
  environment_short       = var.environment_short
  project_name_prefix     = var.project_name_prefix
  main_router_name        = local.main_router_name
  public_dns_ips          = var.public_dns_ips
  ssh_keypair_name        = local.keypair_pubkey_name
  ssh_user                = var.ssh_user
  cluster_slug            = var.galera_slug
  cluster_network_cidr    = var.galera_network_cidr
  cluster_instance        = var.galera_instance
  cluster_instance_groups = var.galera_instance_groups
  cluster_sec_rules       = var.galera_sec_rules
}

# MariaDB staging node
module "mariadb_staging" {
  source                  = "./modules/generic-cluster"
  enabled                 = terraform.workspace == "staging" && var.environment == "staging"
  environment_short       = var.environment_short
  project_name_prefix     = var.project_name_prefix
  main_router_name        = local.main_router_name
  public_dns_ips          = var.public_dns_ips
  ssh_keypair_name        = local.keypair_pubkey_name
  ssh_user                = var.ssh_user
  cluster_slug            = var.mariadb_slug
  cluster_network_cidr    = var.mariadb_network_cidr
  cluster_instance        = var.mariadb_instance
  cluster_instance_groups = var.mariadb_instance_groups
  cluster_sec_rules       = var.mariadb_sec_rules
}

# Galera public playground nodes
module "galera_public-playground" {
  source                  = "./modules/generic-cluster"
  enabled                 = terraform.workspace == "public-playground" && var.environment == "public-playground"
  environment_short       = var.environment_short
  project_name_prefix     = var.project_name_prefix
  main_router_name        = local.main_router_name
  public_dns_ips          = var.public_dns_ips
  ssh_keypair_name        = local.keypair_pubkey_name
  ssh_user                = var.ssh_user
  cluster_slug            = var.galera_slug
  cluster_network_cidr    = var.galera_network_cidr
  cluster_instance        = var.galera_instance
  cluster_instance_groups = var.galera_instance_groups
  cluster_sec_rules       = var.galera_sec_rules
}

module "lb_elastic" {
  source                = "./modules/load-balancer"
  enabled               = terraform.workspace == "production" && var.environment == "production"
  lb_name               = "wai-prod-elastic-lb"
  lb_subnet_id          = module.elastic.out_subnet_id
  lb_ports              = var.elastic_load_balancer_ports
  lb_members            = module.elastic.out_members_access_ip_v4
  lb_security_group_ids = module.elastic.out_security_group_ids
}

module "lb_galera_play" {
  source                = "./modules/load-balancer"
  enabled               = terraform.workspace == "public-playground" && var.environment == "public-playground"
  lb_name               = "wai-play-galera-lb"
  lb_subnet_id          = module.galera_public-playground.out_subnet_id
  lb_ports              = var.galera_load_balancer_ports
  lb_members            = module.galera_public-playground.out_members_access_ip_v4
  lb_security_group_ids = module.galera_public-playground.out_security_group_ids
}

module "lb_galera_prod" {
  source                = "./modules/load-balancer"
  enabled               = terraform.workspace == "production" && var.environment == "production"
  lb_name               = "wai-prod-galera-master-lb"
  lb_subnet_id          = module.galera_production.out_subnet_id
  lb_ports              = var.galera_load_balancer_ports
  lb_members            = slice(module.galera_production.out_members_access_ip_v4, 0, length(module.galera_production.out_members_access_ip_v4) / 2)
  lb_security_group_ids = module.galera_production.out_security_group_ids
}

module "lb_galera_prod_slave" {
  source                = "./modules/load-balancer"
  enabled               = terraform.workspace == "production" && var.environment == "production"
  lb_name               = "wai-prod-galera-slave-lb"
  lb_subnet_id          = module.galera_production.out_subnet_id
  lb_ports              = var.galera_load_balancer_ports
  lb_members            = slice(module.galera_production.out_members_access_ip_v4, length(module.galera_production.out_members_access_ip_v4) / 2, length(module.galera_production.out_members_access_ip_v4))
  lb_security_group_ids = module.galera_production.out_security_group_ids
}

module "lb_k8s_worker" {
  source                = "./modules/load-balancer"
  enabled               = terraform.workspace == "production" && var.environment == "production"
  lb_name               = "wai-k8s-worker-lb"
  lb_subnet_id          = module.kubernetes.out_subnet_id
  lb_ports              = var.k8s_worker_load_balancer_ports
  lb_members            = module.kubernetes.out_members_access_ip_v4
  lb_security_group_ids = module.kubernetes.out_security_group_ids
}