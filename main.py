from typing import Any, Callable

from cloud_functions.sandbox_data_delivery import (
    copy_sandbox_dd_files_to_dev as _copy_sandbox_dd_files_to_dev,
)

_copy_sandbox_dd_files_to_dev_typed: Callable[[dict[str, Any], Any], Any] = (
    _copy_sandbox_dd_files_to_dev
)


def copy_sandbox_dd_files_to_dev(data: dict[str, Any], context: Any) -> Any:
    return _copy_sandbox_dd_files_to_dev_typed(data, context)
