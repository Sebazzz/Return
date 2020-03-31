#!/bin/bash

function ensure_success {
	exit_status=$?
	if [ $exit_status -ne 0 ]; then
		echo "... command exited with code $exit_status"
		exit $exit_status
	fi
}

echo "Install node.js prerequisites"
apt-get -qq update && apt-get -qqy --no-install-recommends install wget gnupg git unzip
ensure_success

echo "Install node.js itself"
curl -sL https://deb.nodesource.com/setup_12.x | bash -
ensure_success

apt-get install --no-install-recommends -y gcc g++ make build-essential nodejs
ensure_success
