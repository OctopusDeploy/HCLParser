provider "google" {
  region  = "us-central1"
  project = "default-project-160900"
}

resource "random_id" "trainee" {
  count       = "${length(var.trainees)}"
  byte_length = "4"
}

resource "google_project" "trainee" {
  count           = "${length(var.trainees)}"
  name            = "trainee-${element(random_id.trainee.*.hex, count.index)}"
  project_id      = "trainee-${element(random_id.trainee.*.hex, count.index)}"
  billing_account = "${var.billing_id}"
  org_id          = "${var.org_id}"
}

data "google_iam_policy" "trainee" {
  count = "${length(var.trainees)}"

  binding = {
    role = "roles/owner"

    members = [
      "user:seth@sethvargo.com",
    ]
  }

  binding = {
    role = "roles/editor"

    members = [
      "user:${element(var.trainees, count.index)}",
    ]
  }
}

resource "google_project_iam_policy" "trainee" {
  count       = "${length(var.trainees)}"
  project     = "${element(google_project.trainee.*.project_id, count.index)}"
  policy_data = "${element(data.google_iam_policy.trainee.*.policy_data, count.index)}"
}

resource "google_project_services" "trainee" {
  count   = "${length(var.trainees)}"
  project = "${element(google_project.trainee.*.project_id, count.index)}"

  services = [
    "containerregistry.googleapis.com",
    "pubsub.googleapis.com",
    "deploymentmanager.googleapis.com",
    "replicapool.googleapis.com",
    "replicapoolupdater.googleapis.com",
    "resourceviews.googleapis.com",
    "compute-component.googleapis.com",
    "container.googleapis.com",
    "storage-api.googleapis.com",
  ]
}

resource "google_compute_instance" "trainee" {
  count        = "${length(var.trainees)}"
  name         = "default"
  machine_type = "n1-standard-1"
  zone         = "us-central1-a"

  can_ip_forward = true

  project = "${element(google_project_services.trainee.*.project, count.index)}"

  disk {
    image = "debian-cloud/debian-8"
  }

  disk {
    type    = "local-ssd"
    scratch = true
  }

  metadata {
    ssh-keys = "root:${file("~/.ssh/id_rsa.pub")}"
  }

  network_interface {
    network = "default"

    access_config {}
  }
}
