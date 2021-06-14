#!/bin/bash -l
#SBATCH -C cgpu01
#SBATCH -c 6
#SBATCH --mem=6G
#SBATCH --partition=short
#SBATCH --time=02:00:00
#SBATCH --job-name=ml-agents-job
#SBATCH --mail-user=smttroet@tu-dortmund.de
#SBATCH --mail-type=ALL
#SBATCH --output=goalkeeper.log
#SBATCH --signal=B:SIGINT@90


module load python/3.7.7

MLAGENTS_PID=0
RUN_ID=goalkeeper


cleanup_before_exiting() {
	echo 'Got sigquit';
	kill -2 $MLAGENTS_PID;
	echo 'start killing mlagents';	
	wait;
	echo 'killed mlagents';
	exit 0;
}
trap -- 'cleanup_before_exiting' SIGINT

srun -X mlagents-learn configuration.yaml --run-id $RUN_ID &
MLAGENTS_PID=$!
wait
