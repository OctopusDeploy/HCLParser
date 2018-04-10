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
}