provider "aws" {
  region  = "us-west-2"
  profile = "octopuscloud"
}

# Store state data on S3
terraform {
  backend "s3" {
    profile = "octopuscloud"
    bucket  = "octopus-cloud-terraform-state"
    key     = "production-vpc-uswe2/production-vpc-uswe2-terraform.tfstate"
    region  = "us-west-2"
  }
}

# Retrieve state data from S3
data "terraform_remote_state" "state" {
  backend = "s3"
  config {
    profile = "octopuscloud"
    bucket  = "octopus-cloud-terraform-state"
    key     = "production-vpc-uswe2/production-vpc-uswe2-terraform.tfstate"
    region  = "us-west-2"
  }
}

resource "aws_vpc" "main" {
  cidr_block           = "10.11.0.0/16"
  instance_tenancy     = "default"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags {
    Name = "Production VPC"
  }
}

resource "aws_internet_gateway" "main" {
  vpc_id = "${aws_vpc.main.id}"

  tags {
    Name = "Production Internet Gateway"
  }
}

resource "aws_route_table" "public" {
  vpc_id = "${aws_vpc.main.id}"

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = "${aws_internet_gateway.main.id}"
  }

  tags {
    Name = "Production Subnet (Public A/B/C)"
  }
}

# Public A
resource "aws_eip" "public_a" {
  vpc = true
}

resource "aws_subnet" "public_a" {
  vpc_id                  = "${aws_vpc.main.id}"
  cidr_block              = "10.11.0.0/19"
  availability_zone       = "us-west-2a"
  map_public_ip_on_launch = true

  tags {
    Name = "Production Subnet (Public A)"
  }
}

resource "aws_route_table_association" "public_a" {
  subnet_id      = "${aws_subnet.public_a.id}"
  route_table_id = "${aws_route_table.public.id}"
}

resource "aws_nat_gateway" "public_a" {
  allocation_id = "${aws_eip.public_a.id}"
  subnet_id     = "${aws_subnet.public_a.id}"

  tags {
    Name = "Production NAT Gateway (Public A)"
  }
}

# Public B
resource "aws_eip" "public_b" {
  vpc = true
}

resource "aws_subnet" "public_b" {
  vpc_id                  = "${aws_vpc.main.id}"
  cidr_block              = "10.11.32.0/19"
  availability_zone       = "us-west-2b"
  map_public_ip_on_launch = true

  tags {
    Name = "Production Subnet (Public B)"
  }
}

resource "aws_route_table_association" "public_b" {
  subnet_id      = "${aws_subnet.public_b.id}"
  route_table_id = "${aws_route_table.public.id}"
}

resource "aws_nat_gateway" "public_b" {
  allocation_id = "${aws_eip.public_b.id}"
  subnet_id     = "${aws_subnet.public_b.id}"

  tags {
    Name = "Production NAT Gateway (Public B)"
  }
}

# Public C
resource "aws_eip" "public_c" {
  vpc = true
}

resource "aws_subnet" "public_c" {
  vpc_id                  = "${aws_vpc.main.id}"
  cidr_block              = "10.11.64.0/19"
  availability_zone       = "us-west-2c"
  map_public_ip_on_launch = true

  tags {
    Name = "Production Subnet (Public C)"
  }
}

resource "aws_route_table_association" "public_c" {
  subnet_id      = "${aws_subnet.public_c.id}"
  route_table_id = "${aws_route_table.public.id}"
}

resource "aws_nat_gateway" "public_c" {
  allocation_id = "${aws_eip.public_c.id}"
  subnet_id     = "${aws_subnet.public_c.id}"

  tags {
    Name = "Production NAT Gateway (Public C)"
  }
}

# Private A
resource "aws_subnet" "private_a" {
  vpc_id            = "${aws_vpc.main.id}"
  cidr_block        = "10.11.128.0/19"
  availability_zone = "us-west-2a"

  tags {
    Name = "Production Subnet (Private A)"
  }
}

resource "aws_route_table" "private_a" {
  vpc_id = "${aws_vpc.main.id}"

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = "${aws_nat_gateway.public_a.id}"
  }

  tags {
    Name = "Production Subnet (Private A)"
  }
}

resource "aws_route_table_association" "private_a" {
  subnet_id      = "${aws_subnet.private_a.id}"
  route_table_id = "${aws_route_table.private_a.id}"
}

# Private B
resource "aws_subnet" "private_b" {
  vpc_id            = "${aws_vpc.main.id}"
  cidr_block        = "10.11.160.0/19"
  availability_zone = "us-west-2b"

  tags {
    Name = "Production Subnet (Private B)"
  }
}

resource "aws_route_table" "private_b" {
  vpc_id = "${aws_vpc.main.id}"

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = "${aws_nat_gateway.public_b.id}"
  }

  tags {
    Name = "Production Subnet (Private B)"
  }
}

resource "aws_route_table_association" "private_b" {
  subnet_id      = "${aws_subnet.private_b.id}"
  route_table_id = "${aws_route_table.private_b.id}"
}

# Private C
resource "aws_subnet" "private_c" {
  vpc_id            = "${aws_vpc.main.id}"
  cidr_block        = "10.11.192.0/19"
  availability_zone = "us-west-2c"

  tags {
    Name = "Production Subnet (Private C)"
  }
}

resource "aws_route_table" "private_c" {
  vpc_id = "${aws_vpc.main.id}"

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = "${aws_nat_gateway.public_c.id}"
  }

  tags {
    Name = "Production Subnet (Private C)"
  }
}

resource "aws_route_table_association" "private_c" {
  subnet_id      = "${aws_subnet.private_c.id}"
  route_table_id = "${aws_route_table.private_c.id}"
}

resource "aws_default_network_acl" "main" {
  default_network_acl_id = "${aws_vpc.main.default_network_acl_id}"

  ingress {
    rule_no    = 100
    protocol   = "tcp"
    from_port  = 80
    to_port    = 80
    cidr_block = "0.0.0.0/0"
    action     = "allow"
  }
  ingress {
    rule_no    = 110
    protocol   = "tcp"
    from_port  = 443
    to_port    = 443
    cidr_block = "0.0.0.0/0"
    action     = "allow"
  }
  ingress {
    rule_no    = 120
    protocol   = "tcp"
    from_port  = 1433
    to_port    = 1433
    cidr_block = "10.11.0.0/16"
    action     = "allow"
  }
  ingress {
    rule_no    = 121
    protocol   = "tcp"
    from_port  = 1433
    to_port    = 1433
    cidr_block = "10.112.0.0/16" #todo: old mgmt vpc, will need to change once new mgmt vpc is ready
    action     = "allow"
  }
  ingress {
    rule_no    = 130
    protocol   = "tcp"
    from_port  = 22
    to_port    = 22
    cidr_block = "10.112.0.0/16" #todo: old mgmt vpc, will need to change once new mgmt vpc is ready
    action     = "allow"
  }
  ingress {
    rule_no    = 140
    protocol   = "tcp"
    from_port  = 3389
    to_port    = 3389
    cidr_block = "10.112.0.0/16" #todo: old mgmt vpc, will need to change once new mgmt vpc is ready
    action     = "allow"
  }
  ingress { # Listening Tentacle
    rule_no    = 150
    protocol   = "tcp"
    from_port  = 10933
    to_port    = 10933
    cidr_block = "10.112.0.0/16" #todo: old mgmt vpc, will need to change once new mgmt vpc is ready
    action     = "allow"
  }
  ingress { # Polling Tentacle
    rule_no    = 151
    protocol   = "tcp"
    from_port  = 10943
    to_port    = 10943
    cidr_block = "0.0.0.0/0"
    action     = "allow"
  }
  ingress { # Ephemeral Ports
    rule_no    = 160
    protocol   = "tcp"
    from_port  = 1024
    to_port    = 65535
    cidr_block = "0.0.0.0/0"
    action     = "allow"
  }
  ingress {
    rule_no    = 170
    protocol   = "udp"
    from_port  = 123
    to_port    = 123
    cidr_block = "0.0.0.0/0"
    action     = "allow"
  }

  egress {
    rule_no    = 100
    protocol   = "tcp"
    from_port  = 445
    to_port    = 445
    cidr_block = "0.0.0.0/0"
    action     = "deny"
  }
  egress {
    rule_no    = 110
    protocol   = "tcp"
    from_port  = 3389
    to_port    = 3389
    cidr_block = "0.0.0.0/0"
    action     = "deny"
  }
  egress { # WinRM over HTTP
    rule_no    = 120
    protocol   = "tcp"
    from_port  = 5985
    to_port    = 5985
    cidr_block = "0.0.0.0/0"
    action     = "deny"
  }
  egress {
    rule_no    = 130
    protocol   = "-1"
    from_port  = 0
    to_port    = 0
    cidr_block = "0.0.0.0/0"
    action     = "allow"
  }

  tags {
    Name = "Production Network ACL"
  }
}

resource "aws_vpc_endpoint" "s3" {
  vpc_id       = "${aws_vpc.main.id}"
  service_name = "com.amazonaws.us-west-2.s3"
  policy       = <<POLICY
{
  "Statement": [
    {
      "Action": "*",
      "Effect": "Allow",
      "Resource": "*",
      "Principal": "*"
    }
  ]
}
POLICY
  route_table_ids = [
    "${aws_route_table.private_a.id}",
    "${aws_route_table.private_b.id}",
    "${aws_route_table.private_c.id}"
  ]
}

resource "aws_security_group" "main" {
  name        = "MainProductionOctopusCloudInstanceSecurityGroup"
  description = "Main Production Octopus Cloud Instance Security Group"
  vpc_id      = "${aws_vpc.main.id}"

  ingress {
    protocol    = "tcp"
    from_port   = 80
    to_port     = 80
    cidr_blocks = [
      "0.0.0.0/0"
    ]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 443
    to_port     = 443
    cidr_blocks = [
      "0.0.0.0/0"
    ]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 3389
    to_port     = 3389
    cidr_blocks = [
      "10.112.0.0/16" #todo: old mgmt vpc, will need to change once new mgmt vpc is ready
    ]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 10933
    to_port     = 10933
    cidr_blocks = [
      "10.112.0.0/16" #todo: old mgmt vpc, will need to change once new mgmt vpc is ready
    ]
  }

  ingress {
    protocol    = "tcp"
    from_port   = 10943
    to_port     = 10943
    cidr_blocks = [
      "0.0.0.0/0"
    ]
  }

  egress {
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_blocks = [
      "0.0.0.0/0"
    ]
  }

  tags {
    Name = "Main Production Octopus Cloud Instance Security Group"
  }
}
