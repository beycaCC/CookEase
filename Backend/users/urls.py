from django.urls import path
from . import views

urlpatterns = [
    path("get_user_ingredients/", views.get_user_ingredients, name="get_user_ingredients")
]