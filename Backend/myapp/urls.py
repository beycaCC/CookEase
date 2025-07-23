# Place URL routes and connect them to views
from django.urls import path
from . import views

urlpatterns = [
    path("", views.home, name="home")
]