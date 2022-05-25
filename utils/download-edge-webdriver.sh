#!/bin/bash

function ensure_success {
	exit_status=$?
	if [ $exit_status -ne 0 ]; then
		echo "... command exited with code $exit_status"
		exit $exit_status
	fi
}

curl -L -o build/msedgedriver.zip "https://msedgedriver.azureedge.net/$(microsoft-edge-stable --version | egrep -oih '([0-9]+\.?)+')/edgedriver_linux64.zip"
ensure_success