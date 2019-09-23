#!/bin/bash

if [ -z "$He_Name" ]
then
  echo "Please set He_Name before running this script"
else
  if [ -f ~/${He_Name}.env ]
  then
    if [ "$#" = 0 ] || [ $1 != "-y" ]
    then
      read -p "~/${He_Name}.env already exists. Do you want to remove? (y/n) " response

      if ! [[ $response =~ [yY] ]]
      then
        echo "Please move or delete ~/${He_Name}.env and rerun the script."
        exit 1;
      fi
    fi
  fi
  echo '#!/bin/bash' > ~/${He_Name}.env
  echo '' >> ~/${He_Name}.env

  for var in $(env | grep He_ | sort)
  do
    echo "export ${var}" >> ~/${He_Name}.env
  done

  cat ~/${He_Name}.env
fi
