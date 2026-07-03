.PHONY: lint lint-fix typecheck deptry vulture test

lint:
	poetry run ruff check .

lint-fix:
	poetry run ruff check --fix .
	poetry run ruff format .

typecheck:
	poetry run pyright

deptry:
	poetry run deptry . --extend-exclude "cloud_functions/test_"

vulture:
	poetry run vulture .

test:
	poetry run pytest
