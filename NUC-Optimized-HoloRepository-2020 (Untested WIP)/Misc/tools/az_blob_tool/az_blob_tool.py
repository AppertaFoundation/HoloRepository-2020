#!/usr/bin/env python

import os
import sys
import json
import uuid
import glob
import fire
from azure.storage.blob import BlockBlobService, PublicAccess


class Holoblob:
    """A tool to manage data in azure holoblob storage"""

    def __init__(self, configfile: str = "config.cfg"):
        """Create BlockBlockService object to access the Azure Blob service"""
        if configfile:
            with open(configfile, "r") as f:
                configs = json.load(f)
        else:
            sys.exit("Config file not found")

        self._service = BlockBlobService(
            account_name=configs["account_name"], account_key=configs["account_key"]
        )

    def create_container(self, container_name: str, public: bool = False):
        """Creates containers within storage account"""
        try:
            access = None
            if public:
                access = PublicAccess.Container

            self._service.create_container(container_name, public_access=access)
            print(f"Container {container_name} to be created")

        except Exception as e:
            print(e)

    def upload_file(self, container_name: str, file_path: str, add_uuid: bool = False):
        """Upload a single file, use local_file_name for the blob name"""
        try:
            name, ext = os.path.splitext(os.path.basename(file_path))
            if add_uuid:
                local_file_name = name + "-" + str(uuid.uuid4()) + ext
            else:
                local_file_name = os.path.basename(file_path)
            self._service.create_blob_from_path(
                container_name, local_file_name, file_path
            )
            print(f"{file_path} uploaded to {container_name} as {local_file_name}")
            print(
                f"Access-URL: {self._service.make_blob_url(container_name, local_file_name)}"
            )

        except Exception as e:
            print(e)

    def upload_folder(
        self, container_name: str, src_dir: str, ext: str = "", add_uuid: bool = False
    ):
        """Upload all contents in a folder to a blob container"""

        print(f"Uploading all files in '{src_dir}' to '{container_name}'")
        search_path = os.path.join(src_dir, "*")
        if ext:
            search_path = os.path.join(src_dir, "*." + ext)
        for blob in glob.glob(search_path):
            self.upload_file(container_name, blob, add_uuid)

    def list_blobs(self, container_name: str):
        """List all files within a container"""
        try:
            generator = self._service.list_blobs(container_name)
            for blob in generator:
                print("\t Blob name: " + blob.name)

        except Exception as e:
            print(e)

    def list_containers(self):
        """List all containers in the blob storage"""
        try:
            containers = self._service.list_containers()
            for container in containers:
                print("\t Container name: " + container.name)
        except Exception as e:
            print(e)

    def delete_blob(self, container_name: str, filename: str):
        """Delete a blob within a container"""
        try:
            self._service.delete_blob(container_name, filename)
            print(f"Successful: {filename} deleted")

        except Exception as e:
            print(e)

    def delete_container(self, container_name: str):
        """Triggers Azure to start the deletion of the container from storage

        This operation may not be instant
        """
        try:
            self._service.delete_container(container_name)
            print(f"Successful: {container_name} sent for deletion")

        except Exception as e:
            print(e)


# Main method.
if __name__ == "__main__":
    fire.Fire(Holoblob)
