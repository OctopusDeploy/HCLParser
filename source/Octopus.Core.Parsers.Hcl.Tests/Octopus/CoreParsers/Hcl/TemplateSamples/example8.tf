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