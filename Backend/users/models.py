from django.db import models

# Create your models here.
class User(models.Model):
    user_id = models.AutoField(primary_key=True)
    username = models.CharField(max_length=255)
    email = models.EmailField(unique=True)
    password = models.CharField()
    created_at = models.DateTimeField(auto_now_add=True)

class UserIngredient(models.Model):
    user_id = models.ForeignKey(User, on_delete=models.CASCADE)
    i_id = models.ForeignKey(Ingredient, on_delete=models.CASCADE)
    ciid = models.ForeignKey(CustomizedIngredient, on_delete=models.CASCADE)
    name = models.CharField(max_length=100)
    quantity = models.CharField(max_length=50)
    expiration_day = models.DateTimeField(auto_now_add=True)

# Should be pre-defined based on dataset and not be changed
class Ingredient(models.Model):
    i_id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=100)
    general_expiration_date = models.PositiveIntegerField()

class CustomizedIngredient(models.Model):
    ciid = models.ForeignKey(Ingredient, on_delete=models.CASCADE)
    name = models.CharField(max_length=100)
    expiration_date = models.DateTimeField(auto_now_add=True)


