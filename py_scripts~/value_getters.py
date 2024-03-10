from os import environ
import random


def get_str(field_name):
  return environ.get(field_name)


def get_float(field_name):
  return float(environ.get(field_name))


def get_int(field_name):
  return int(get_float(field_name))


if (get_str("seed") != None and get_int("seed") > 0):
  random.seed(get_int("seed"))


def get_vec(field_name_x, field_name_y, field_name_z):
  return (get_float(field_name_x), get_float(field_name_y),
          get_float(field_name_z))


def get_bool(field_name):
  value = environ.get(field_name)
  return value == 'True' or value == 'true' or value == "T" or value == 't'


def get_int_in_range(field_name):
  num1, num2 = map(int, get_str(field_name).split(','))

  return random.randint(num1, num2)


def get_float_in_range(field_name):
  num1, num2 = map(float, get_str(field_name).split(','))
  return random.uniform(num1, num2)
