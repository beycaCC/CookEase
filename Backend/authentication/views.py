from django.shortcuts import render, HttpResponse

# Create your views here.
def auth(request):
    return HttpResponse("User Auth")