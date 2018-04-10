#
# Provider. We assume access keys are provided via environment variables.
#

provider "aws" {
  region = "${var.aws_region}"
}