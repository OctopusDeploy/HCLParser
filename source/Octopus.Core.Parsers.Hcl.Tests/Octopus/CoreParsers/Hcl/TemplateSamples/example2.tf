provider "aws" {
  region  = "us-west-2"
  profile = "octopuscloud"
}

# Store the state as a given key in a given bucket on Amazon S3
terraform {
  backend "s3" {
    profile = "octopuscloud"
    bucket  = "octopus-cloud-terraform-state"
    key     = "openvpn-uswe2/openvpn-uswe2-terraform.tfstate"
    region  = "us-west-2"
  }
}

# Retrieve state meta data from a given bucket on Amazon S3
data "terraform_remote_state" "state" {
  backend = "s3"
  config {
    profile = "octopuscloud"
    bucket  = "octopus-cloud-terraform-state"
    key     = "openvpn-uswe2/openvpn-uswe2-terraform.tfstate"
    region  = "us-west-2"
  }
}

# Transform UserData Variables
data "template_file" "userdata" {
  template = "${file("userdata.tpl")}"

  vars {
    public_hostname = "${var.openvpn_public_hostname}"
    admin_user      = "${var.openvpn_admin_user}"
    admin_pw        = "${var.openvpn_admin_pw}"
    reroute_gw      = 0
    reroute_dns     = 0
  }
}

# Create Security Group
resource "aws_security_group" "openvpn" {
  name          = "OpenVPN"
  description   = "OpenVPN Security Group"
  vpc_id        = "${var.aws_vpc_id}"

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["${var.your_public_ip_address}/32"]
  }

  # Required for Let's Encrypt
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 943
    to_port     = 943
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 1194
    to_port     = 1194
    protocol    = "udp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags {
    Name = "OpenVPN"
  }
}

# Create IAM Role
resource "aws_iam_role" "openvpn" {
  name               = "OpenVPN"
  path               = "/"
  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "ec2.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

# Create IAM Instance Profile
resource "aws_iam_instance_profile" "openvpn" {
  name  = "OpenVPN"
  role  = "${aws_iam_role.openvpn.name}"
}

# Create IAM Policy
resource "aws_iam_role_policy" "openvpn_s3" {
  name = "S3-AllowAll-hosted-backup-us-west-2"
  role = "${aws_iam_role.openvpn.id}"
  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "s3:*"
      ],
      "Effect": "Allow",
      "Resource": [
        "arn:aws:s3:::hosted-backup-us-west-2",
        "arn:aws:s3:::hosted-backup-us-west-2/*"
      ]
    }
  ]
}
EOF
}

# Create EC2 Instance
resource "aws_instance" "openvpn" {
  connection {
    user        = "openvpnas"
    private_key = "${file(var.private_key_path)}"
  }

  instance_type               = "${var.instance_type}"
  ami                         = "${var.instance_ami}"
  subnet_id                   = "${var.aws_subnet_id}"
  key_name                    = "${var.private_key_name}"
  vpc_security_group_ids      = ["${aws_security_group.openvpn.id}"]
  iam_instance_profile        = "${aws_iam_instance_profile.openvpn.id}"
  user_data                   = "${data.template_file.userdata.rendered}"

  provisioner "file" {
    source      = "backup.sh"
    destination = "/home/openvpnas/backup.sh"
  }

  provisioner "file" {
    source      = "restore.sh"
    destination = "/home/openvpnas/restore.sh"
  }

  provisioner "remote-exec" {
    inline = [
      "sudo apt-get -y update",
      "sudo apt-get -y install awscli",
      "sudo apt-get -y install software-properties-common",
      "sudo add-apt-repository -y ppa:certbot/certbot",
      "sudo apt-get -y update",
      "sudo apt-get -y install certbot",
      "sudo service openvpnas stop",
      "sudo certbot certonly --standalone --non-interactive --agree-tos --email ${var.lets_encrypt_email} --domains ${var.openvpn_public_hostname} --pre-hook 'sudo service openvpnas stop' --post-hook 'sudo service openvpnas start'",
      "sudo ln -s -f /etc/letsencrypt/live/${var.openvpn_public_hostname}/cert.pem /usr/local/openvpn_as/etc/web-ssl/server.crt",
      "sudo ln -s -f /etc/letsencrypt/live/${var.openvpn_public_hostname}/privkey.pem /usr/local/openvpn_as/etc/web-ssl/server.key",
      "sudo service openvpnas start",
      "chmod u+x /home/openvpnas/backup.sh",
      "chmod u+x /home/openvpnas/restore.sh",
      "{ sudo crontab -l; echo '0 15 * * * certbot renew'; } | sudo crontab -"
      "{ sudo crontab -l; echo '0 16 * * * /home/openvpnas/backup.sh'; } | sudo crontab -"
    ]
  }

  tags {
    Name = "OpenVPN Access Server"
  }
}

# Allocate an Elastic IP to the EC2 Instance
resource "aws_eip" "openvpn" {
  instance = "${aws_instance.openvpn.id}"
  vpc      = true
}

# Create Route53 Record
resource "aws_route53_record" "www" {
  zone_id = "${var.r53_zone_id}"
  name    = "${var.r53_name}"
  type    = "A"
  ttl     = "300"
  records = ["${aws_eip.openvpn.public_ip}"]
}
