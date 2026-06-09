import logging
import os
from typing import TYPE_CHECKING

import google.cloud.storage as storage
from google.cloud.logging_v2.handlers import StructuredLogHandler, setup_logging

if not TYPE_CHECKING:
    handler = StructuredLogHandler()
    setup_logging(handler)


def copy_sandbox_dd_files_to_dev(data, _context):

    logging.info(f"Sandbox data delivery process triggered")
    try:
        if not data:
            raise ValueError("Not a valid request object")

        bucket_name = data["bucket"]
        file_name = data["name"]

        logging.info(f"File received: {file_name}")

        if file_name.startswith("dd_"):

            storage_client = storage.Client()

            destination_bucket_name = "ons-blaise-v2-dev-nifi"

            env_suffix = get_environment_suffix(bucket_name)
            filename, fileExtension = os.path.splitext(
                file_name
            )  # Splits at extension only

            prefix, suffix = split_filename(filename)

            new_file_name = f"{prefix}_sandbox_{env_suffix}_{suffix}{fileExtension}"

            source_bucket = storage_client.bucket(bucket_name)
            destination_bucket = storage_client.bucket(destination_bucket_name)

            source_blob = source_bucket.blob(file_name)

            source_bucket.copy_blob(source_blob, destination_bucket, new_file_name)

            logging.info(
                f"File {file_name} copied to {destination_bucket_name} renamed as {new_file_name}"
            )
        else:
            logging.info("Non-dd file received, no data delivery needed")
            return
    except Exception as e:
        error = f"An error occured while trying to run the sandbox data delivery function. Exception: {e}"
        logging.error(error)
        return error, 500


def get_environment_suffix(bucket_name):
    parts = bucket_name.split("-")
    env_suffix = parts[len(parts) - 2]
    return env_suffix


def split_filename(filename):
    filename = "".join(filename)

    parts = filename.rsplit("_", 2)
    if len(parts) == 3:
        prefix = parts[0]
        suffix = "_".join(parts[1:])
        return prefix, suffix
    return None, None
