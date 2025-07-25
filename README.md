# CookEase

A mobile application that could change your cooking life.

## 🚀 Project Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/your-repo.git
cd your-repo
```

### 2. Create a Virtual Environment

```bash
python3 -m venv .venv
source .venv/bin/activate  # On Windows: .venv\Scripts\activate
```

### 3. Install Dependencies

```bash
cd Backend/
pip install -r requirements.txt
```

(use `pip freeze > requirements.txt` later to update the requirements.txt list)

### 4. Set Up PostgreSQL Database

- Ensure PostgreSQL is running.
- Create a PostgreSQL database manually if it doesn't exist.
- Update the `DATABASES` section in `yourproject/settings.py`:

```python
DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.postgresql',
        'NAME': 'your_db_name',
        'USER': 'your_db_user',
        'PASSWORD': 'your_db_password',
        'HOST': 'localhost',
        'PORT': '5432',
    }
}
```

### 5. Apply Migrations

```bash
python manage.py migrate
```

### 6. Create a Superuser (Optional)

```bash
python manage.py createsuperuser
```

### 7. Run the Development Server

```bash
python manage.py runserver
```
