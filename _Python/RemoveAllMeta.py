import os

def remove_meta_files_and_empty_dirs(root_dir):
	for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
		# Xoá file .meta
		for filename in filenames:
			if filename.endswith('.meta'):
				file_path = os.path.join(dirpath, filename)
				try:
					os.remove(file_path)
					print(f"Đã xoá file: {file_path}")
				except Exception as e:
					print(f"Không thể xoá file {file_path}: {e}")
		# Xoá folder rỗng
		if dirpath != root_dir:
			try:
				if not os.listdir(dirpath):
					os.rmdir(dirpath)
					print(f"Đã xoá folder rỗng: {dirpath}")
			except Exception as e:
				print(f"Không thể xoá folder {dirpath}: {e}")

if __name__ == "__main__":
	folder = os.path.dirname(os.path.abspath(__file__))
	remove_meta_files_and_empty_dirs(folder)
