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