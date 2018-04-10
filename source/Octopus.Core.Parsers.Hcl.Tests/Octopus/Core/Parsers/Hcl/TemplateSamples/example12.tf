# Create EC2 Instance
resource "aws_instance" "openvpn" {
  provisioner "remote-exec" {
    tags {
      Name = "OpenVPN Access Server"
    }
  }
}