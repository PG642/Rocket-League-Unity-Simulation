#!/bin/bash -l
#SBATCH -C cgpu01
#SBATCH -c 6
#SBATCH --mem=12G
#SBATCH --partition=short
#SBATCH --time=02:00:00
#SBATCH --job-name=ml-agents-job
#ADD MAIL
#SBATCH --mail-user=
#SBATCH --mail-type=ALL
#SBATCH --output=airdribble.log
#SBATCH --signal=B:SIGINT@90


module load python/3.7.7

MLAGENTS_PID=0
RUN_ID=airdribble


cleanup_before_exiting() {
	echo 'Got sigquit';
	kill -2 $MLAGENTS_PID;
	echo 'start killing mlagents';	
	wait;
	echo 'killed mlagents';
	exit 0;
}
trap -- 'cleanup_before_exiting' SIGINT

srun -X mlagents-learn configuration.yaml --run-id $RUN_ID --force &
MLAGENTS_PID=$!
wait
