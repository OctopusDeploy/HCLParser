# Create IAM Instance Profile
resource "aws_iam_instance_profile" "openvpn" {
  name  = "OpenVPN"
  role  = "${aws_iam_role.openvpn.name}"
}