#!/bin/bash

export He_Repo=helium-java
export AUTH_TYPE=CLI

if [ -z "$He_Name" ]
then
  echo "Please set He_Name before running this script"
else
  if [ -f ~/.helium.env ]
  then
    if [ "$#" = 0 ] || [ $1 != "-y" ]
    then
      read -p "helium.env already exists. Do you want to remove? (y/n) " response

      if ! [[ $response =~ [yY] ]]
      then
        echo "Please move or delete ~/.helium.env and rerun the script."
        exit 1;
      fi
    fi
  fi

  export KEYVAULT_NAME=$He_Name

  echo '#!/bin/bash' > ~/.helium.env
  echo '' >> ~/.helium.env

  IFS=$'\n'

  for var in $(env | grep -E 'He_|MSI_|AKS_|Imdb_|AUTH_TYPE|KEYVAULT_NAME' | sort | sed "s/=/='/g")
  do
    echo "export ${var}'" >> ~/.helium.env
  done

  cat ~/.helium.env
fi
