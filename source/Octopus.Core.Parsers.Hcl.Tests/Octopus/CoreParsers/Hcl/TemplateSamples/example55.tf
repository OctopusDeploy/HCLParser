provider "aws" {
  region = "us-west-2"
}
resource "aws_instance" "demo" {
    ami = "ami-aa5ebdd2"
    instance_type = "t2.micro"
    count = 1
    key_name = "javahome"
    tags{
      Name = "CreatedByTerraform"
    }
}
