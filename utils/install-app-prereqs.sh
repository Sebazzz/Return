#!/bin/bash

function ensure_success {
	exit_status=$?
	if [ $exit_status -ne 0 ]; then
		echo "... command exited with code $exit_status"
		exit $exit_status
	fi
}

echo "Installing app prerequisites..."
apt-get -qqy update
apt-get -qqy install libgdiplus curl
ensure_success