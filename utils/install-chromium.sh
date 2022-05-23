#!/bin/bash

function ensure_success {
	exit_status=$?
	if [ $exit_status -ne 0 ]; then
		echo "... command exited with code $exit_status"
		exit $exit_status
	fi
}

echo "Installing Edge prerequisites..."
apt-get -qqy update
apt-get -qqy install lsb-release libappindicator3-1
ensure_success

echo "Downloading MS Edge debian package..."
curl -L -o msedge.deb http://packages.microsoft.com/repos/edge/pool/main/m/microsoft-edge-stable/microsoft-edge-stable_100.0.1185.29-1_amd64.deb
ensure_success

echo "Installing MS Edge debian package..."
apt -y install ./msedge.deb
ensure_success

echo "Clean-up"
rm msedge.deb