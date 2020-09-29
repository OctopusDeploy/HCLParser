data "template_file" "registration" {

  depends_on = [aws_s3_bucket.bucket]
}